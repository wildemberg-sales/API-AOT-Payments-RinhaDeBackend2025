namespace ApiPayments.RequisitionsModels.Payloads
{
    public sealed record PaymentPayloadModel(Guid correlationId, float amount);
    public record PaymentPayloadRequestModel(Guid correlationId, float amount, DateTime requestedAt);
}