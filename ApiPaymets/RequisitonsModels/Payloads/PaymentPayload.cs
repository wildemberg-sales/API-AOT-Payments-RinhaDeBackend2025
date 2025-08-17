namespace ApiPayments.RequisitionsModels.Payloads
{
    public sealed record PaymentPayloadModel(Guid correlationId, decimal amount);
    public record PaymentPayloadRequestModel(Guid correlationId, decimal amount, DateTime requestedAt);
}