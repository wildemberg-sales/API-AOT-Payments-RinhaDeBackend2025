using ApiPaymets.Database;
using ApiPaymets.Database.Entities;
using ApiPaymets.RequisitonsModels.Responses;
using Microsoft.EntityFrameworkCore;

namespace ApiPayment.Services.Impl
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApiDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CreatePaymentAsync(Payment payment)
        {
            try
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in creation: {ex.Message}");
                return false;
            }

            return true;
        }

        public async Task<PaymentSummaryResponse?> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Payments.AsQueryable();

            if (from.HasValue)
                query = query.Where(p => p.CreatedAt >= from);

            if (to.HasValue)
                query = query.Where(p => p.CreatedAt <= to);

            var summaryData = await query
            .GroupBy(p => 1)
            .Select(g => new
            {
                DefaultTotalRequest = g.Count(p => !p.IsFallback),
                FallbackTotalRequest = g.Count(p => p.IsFallback),
                DefaultTotalAmount = g.Where(p => !p.IsFallback).Sum(p => p.Amount),
                FallbackTotalAmount = g.Where(p => p.IsFallback).Sum(p => p.Amount)
            })
            .FirstOrDefaultAsync();

            if (summaryData == null)
            {
                _logger.LogError("Not payments found in database");
                return PaymentSummaryResponse.Create(0, 0, 0, 0);
            }

            PaymentSummaryResponse response = PaymentSummaryResponse.Create(
                defaultTotalRequest: summaryData.DefaultTotalRequest,
                defaultTotalAmount: (float)Math.Round(summaryData.DefaultTotalAmount),
                fallbackTotalRequest: summaryData.FallbackTotalRequest,
                fallbackTotalAmount: (float)Math.Round(summaryData.FallbackTotalAmount)
            );

            return response;
        }
    }
}
