using ApiPayment.Services;
using ApiPayment.Services.Impl;
using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets;
using ApiPaymets.BackgroundServices;
using ApiPaymets.Channels;
using ApiPaymets.Clients;
using ApiPaymets.Clients.Impl;
using ApiPaymets.Configurations;
using ApiPaymets.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

//services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentClient, PaymentClient>();

//singletons
builder.Services.AddSingleton<PaymentChannel>();
builder.Services.AddSingleton<PaymentPersistInFailChannel>();

//background services
builder.Services.AddHostedService<PaymentWorker>();
builder.Services.AddHostedService<PaymentPersistInFailWorker>();

//options
builder.Services.Configure<PaymentsApiExternalSettings>(
    builder.Configuration.GetSection(PaymentsApiExternalSettings.SectionName)
);

// 1. Política de Retry: Tenta 3 vezes com espera exponencial + aleatoriedade (jitter)
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Lida com erros de rede, 5xx e 408
    .WaitAndRetryAsync(
        Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3)
    );

// 2. Política de Circuit Breaker: Abre o circuito após 5 falhas consecutivas
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(1)
    );

// 3. Registra o HttpClient para a API Externa com as políticas
builder.Services.AddHttpClient("PaymentsExternal", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<PaymentsApiExternalSettings>>().Value;
    // A URL base aqui é a principal
    client.BaseAddress = new Uri(settings.UrlDefault);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

var paymentApis = app.MapGroup("payments");

paymentApis.MapPost("/", (PaymentPayloadModel payment, PaymentChannel channel) => 
{ 
    channel.Writer.WriteAsync(payment);
    return Results.Created();
});

paymentApis.MapGet("/payments-summary", async (DateTime from, DateTime to, IPaymentService service) => 
{
    var res = await service.GetPaymentsSummaryAsync(from, to);
    return Results.Ok(res);
});

app.Run();


