using ApiPayment.Services;
using ApiPaymets.Channels;
using ApiPaymets.Clients;
using ApiPaymets.Database.Entities;
using ApiPaymets.RequisitonsModels.Responses;

namespace ApiPaymets.BackgroundServices
{
    public class PaymentWorker : BackgroundService
    {
        private readonly ILogger<PaymentWorker> _logger;
        private readonly PaymentChannel _channel;
        private readonly PaymentPersistInFailChannel _persistInFailChannel;
        private readonly IServiceScopeFactory _scopteFactory;

        public PaymentWorker(IServiceScopeFactory scopteFactory, ILogger<PaymentWorker> logger, PaymentChannel channel, PaymentPersistInFailChannel persistInFailChannel)
        {
            _scopteFactory = scopteFactory;
            _logger = logger;
            _channel = channel;
            _persistInFailChannel = persistInFailChannel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                if (_channel.Reader.TryRead(out var paymentRequest))
                {
                    using (var scope = _scopteFactory.CreateScope())
                    {
                        var client = scope.ServiceProvider.GetRequiredService<IPaymentClient>();

                        PaymentRequisitionResult res = await client.SendPaymentForExternalService(paymentRequest);

                        if (res.IsSuccess)
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                            var pay = Payment.Create(paymentRequest.correlationId, paymentRequest.amount, res.RequestedAt, res.IsFallback);

                            var isCreated = await service.CreatePaymentAsync(pay);

                            if (!isCreated)
                            {
                                _logger.LogInformation("Payment {CorrelationId} is not created", pay.CorrelationId);
                                await _persistInFailChannel.Writer.WriteAsync(pay);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Payment {CorrelationId} is processed successfull", paymentRequest.correlationId);
                            await _channel.Writer.WriteAsync(paymentRequest);
                        }

                    }

                }
            }
        }
    }
}
