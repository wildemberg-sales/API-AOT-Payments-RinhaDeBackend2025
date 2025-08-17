using System.Text.Json.Serialization;

namespace ApiPaymets.RequisitonsModels.Responses
{
    public class PaymentSummaryResponse
    {
        [JsonPropertyName("default")]
        public PaymentSummaryData Default { get; set; } = new();

        [JsonPropertyName("fallback")]
        public PaymentSummaryData Fallback { get; set; } = new();
        public PaymentSummaryResponse() { }

        public static PaymentSummaryResponse Create(int defaultTotalRequest, float defaultTotalAmount, int fallbackTotalRequest, float fallbackTotalAmount)
        {
            return new()
            {
                Default = new PaymentSummaryData
                {
                    totalRequests = defaultTotalRequest,
                    totalAmount = defaultTotalAmount,
                },
                Fallback = new PaymentSummaryData
                {
                    totalRequests = fallbackTotalRequest,
                    totalAmount = fallbackTotalAmount
                }
            };
        }
    }

    public class PaymentSummaryData
    {
        public int totalRequests { get; set; }
        public float totalAmount { get; set; }
    }
}
