using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets.RequisitonsModels.Responses;
namespace ApiPaymets.Clients
{
    public interface IPaymentClient
    {
        Task<PaymentRequisitionResult> SendPaymentForExternalService(PaymentPayloadModel payment);
    }
}
