using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets.RequisitonsModels.Responses;
using System.Text.Json.Serialization;

namespace ApiPaymets
{
    [JsonSerializable(typeof(PaymentSummaryResponse))]
    [JsonSerializable(typeof(PaymentPayloadModel))]
    [JsonSerializable(typeof(PaymentPayloadRequestModel))]
    [JsonSerializable(typeof(PaymentRequisitionResult))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {
    }
}
