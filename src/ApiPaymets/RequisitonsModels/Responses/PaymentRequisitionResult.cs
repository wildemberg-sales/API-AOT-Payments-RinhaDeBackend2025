namespace ApiPaymets.RequisitonsModels.Responses
{
    public record PaymentRequisitionResult(bool IsSuccess, bool IsFallback, DateTime RequestedAt);
}
