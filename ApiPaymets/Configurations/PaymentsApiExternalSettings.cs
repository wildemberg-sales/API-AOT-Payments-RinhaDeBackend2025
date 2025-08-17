namespace ApiPaymets.Configurations
{
    public class PaymentsApiExternalSettings
    {
        public const string SectionName = "ApiPaymentsExternal";

        public string UrlDefault { get; set; } = string.Empty;
        public string UrlFallback { get; set; } = string.Empty;
    }
}
