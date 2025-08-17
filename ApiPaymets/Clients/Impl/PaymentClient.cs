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
            var requestedAt = DateTime.UtcNow;
            var payload = new PaymentPayloadRequestModel(payment.correlationId, payment.amount, requestedAt);
            _logger.LogInformation("Attempting to send payment {correlationId} via default route.", payment.correlationId);

            try
            {
                var defaultClient = _httpClientFactory.CreateClient("PaymentsExternal");

                var response = await defaultClient.PostAsJsonAsync("/payments", payload, AppJsonSerializerContext.Default.PaymentPayloadRequestModel);

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Payment {correlationId} sent successfully via default route.", payment.correlationId);
                return new PaymentRequisitionResult(true, false, requestedAt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Default route failed for {correlationId}. Attempting a single request to the fallback route.", payment.correlationId);
                return await TryFallbackRouteAsync(payload, requestedAt);
            }
        }

        private async Task<PaymentRequisitionResult> TryFallbackRouteAsync(PaymentPayloadRequestModel payload, DateTime requestedAt)
        {
            try
            {
                // Criamos um cliente 'limpo', sem políticas de resiliência.
                var fallbackClient = _httpClientFactory.CreateClient();
                var response = await fallbackClient.PostAsJsonAsync(_settings.UrlFallback + "/payments", payload, AppJsonSerializerContext.Default.PaymentPayloadRequestModel);

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Payment {correlationId} sent successfully via fallback route.", payload.correlationId);
                return new PaymentRequisitionResult(true, true, requestedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback route also failed for {correlationId}. The operation has failed.", payload.correlationId);
                return new PaymentRequisitionResult(false, false, requestedAt);
            }
        }
    }
}
