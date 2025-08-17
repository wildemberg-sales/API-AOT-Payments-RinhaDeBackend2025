namespace ApiPaymets.Database.Entities
{
    public class Payment
    {
        public Guid CorrelationId { get; set; }
        public float Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFallback { get; set; } = false;

        public Payment() { }

        public static Payment Create(Guid correlationId, float amount, DateTime? createdAt, bool? isFallback)
        {
            return new()
            {
                CorrelationId = correlationId,
                Amount = amount,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                IsFallback = isFallback ?? false
            };
        }

    }
}

