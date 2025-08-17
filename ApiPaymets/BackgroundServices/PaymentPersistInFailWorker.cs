
using ApiPayment.Services;
using ApiPaymets.Channels;

namespace ApiPaymets.BackgroundServices
{
    public class PaymentPersistInFailWorker : BackgroundService
    {
        private readonly ILogger<PaymentWorker> _logger;
        private readonly PaymentPersistInFailChannel _channel;
        private readonly IServiceScopeFactory _scopteFactory;

        public PaymentPersistInFailWorker(IServiceScopeFactory scopteFactory, ILogger<PaymentWorker> logger, PaymentPersistInFailChannel channel)
        {
            _scopteFactory = scopteFactory;
            _logger = logger;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                if (_channel.Reader.TryRead(out var payment))
                {
                    using(var scope = _scopteFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                        try
                        {
                            var isCreated = await service.CreatePaymentAsync(payment);

                            if (!isCreated)
                            {
                                _logger.LogError("Failed in database save payment {CorrelationId}", payment.CorrelationId);
                                await Task.Delay(1000);
                                await _channel.Writer.WriteAsync(payment);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed in database save payment {CorrelationId} process", payment.CorrelationId);
                            await _channel.Writer.WriteAsync(payment);
                        }
                    }
                }
            }
        }
    }
}
