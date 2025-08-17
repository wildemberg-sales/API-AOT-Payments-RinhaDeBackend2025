using ApiPaymets.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiPaymets.Database
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentEntity());
            base.OnModelCreating(modelBuilder);
        }
    }
}
