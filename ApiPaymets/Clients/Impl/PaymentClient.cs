using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets.Configurations;
using Microsoft.Extensions.Options;
using ApiPayments;
using ApiPaymets.RequisitonsModels.Responses;

namespace ApiPaymets.Clients.Impl
{
    public class PaymentClient : IPaymentClient
    {
        private readonly PaymentsApiExternalSettings _settings;
        private readonly ILogger<PaymentClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentClient(IOptions<PaymentsApiExternalSettings> settings, ILogger<PaymentClient> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;

            if (string.IsNullOrEmpty(_settings.UrlDefault) || string.IsNullOrEmpty(_settings.UrlFallback))
            {
                throw new InvalidOperationException("A URL e a ApiKey da API de pagamento não foram configuradas.");
            }

            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PaymentRequisitionResult> SendPaymentForExternalService(PaymentPayloadModel payment)
        {
            _logger.LogInformation("Initializing payment request for {correlationId}", payment.correlationId);

            var requestedAt = DateTime.UtcNow;
            var payload = new PaymentPayloadRequestModel(payment.correlationId, payment.amount, requestedAt);

            // Tentativa com o default route
            try
            {
                var sucess = await PostPaymentAsync("PaymentsExternal", payload);
                if (sucess)
                {
                    _logger.LogInformation("Payment {correlationId} send for default route", payment.correlationId);
                    return new PaymentRequisitionResult(true, false, requestedAt);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Send {correlationId} payment failed in default route ", payment.correlationId);
            }

            // Tentativa com o fallback route
            try
            {
                var clientFallback = _httpClientFactory.CreateClient();
                var sucess = await PostPaymentAsync(clientFallback, _settings.UrlFallback, payload);
                if (sucess)
                {
                    _logger.LogInformation("Payment {correlationId} send for fallback route", payment.correlationId);
                    return new PaymentRequisitionResult(true, true, requestedAt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send {correlationId} payment failed in fallback route ", payment.correlationId);
            }
        
            // Nenhuma rota deu certo
            _logger.LogError("Failed Send payment {correlationId}, returning for queue", payment.correlationId.ToString());
            return new PaymentRequisitionResult(false, false, requestedAt);
        }

        private async Task<bool> PostPaymentAsync(string clientName, PaymentPayloadRequestModel payload)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var response = await client.PostAsJsonAsync("/payments", payload, AppJsonSerializerContext.Default.PaymentPayloadRequestModel);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> PostPaymentAsync(HttpClient client, string url, PaymentPayloadRequestModel payload)
        {
            var response = await client.PostAsJsonAsync(url, payload, AppJsonSerializerContext.Default.PaymentPayloadRequestModel);
            return response.IsSuccessStatusCode;
        }
    }
}
