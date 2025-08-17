using ApiPaymets.Database.Entities;
using ApiPaymets.RequisitonsModels.Responses;

namespace ApiPayment.Services
{
    public interface IPaymentService
    {
        Task<bool> CreatePaymentAsync(Payment payment);

        Task<PaymentSummaryResponse> GetPaymentsSummaryAsync(DateTime? from, DateTime? to);
    }
}
