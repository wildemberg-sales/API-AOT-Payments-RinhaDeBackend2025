using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiPaymets.Database.Entities
{
    internal sealed class PaymentEntity : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder
                .ToTable("Payments")
                .HasKey(p => p.CorrelationId);

            builder
                .Property(p => p.Amount)
                .IsRequired();

            builder
                .Property(p => p.CreatedAt)
                .IsRequired();

            builder
                .Property(p => p.IsFallback)
                .IsRequired();
        }
    }
}
