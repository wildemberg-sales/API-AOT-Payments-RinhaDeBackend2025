using ApiPaymets.Database;
using ApiPaymets.Database.Entities;
using ApiPaymets.RequisitonsModels.Responses;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task<PaymentSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null)
        {
            var connection = _context.Database.GetDbConnection();

            var sql = @"
                SELECT
                    COALESCE(COUNT(*) FILTER (WHERE NOT ""IsFallback""), 0) AS ""DefaultTotalRequest"",
                    COALESCE(SUM(""Amount"") FILTER (WHERE NOT ""IsFallback""), 0) AS ""DefaultTotalAmount"",
                    COALESCE(COUNT(*) FILTER (WHERE ""IsFallback""), 0) AS ""FallbackTotalRequest"",
                    COALESCE(SUM(""Amount"") FILTER (WHERE ""IsFallback""), 0) AS ""FallbackTotalAmount""
                FROM ""Payments""
                WHERE (@from IS NULL OR ""CreatedAt"" >= @from)
                  AND (@to IS NULL OR ""CreatedAt"" <= @to);
            ";

            var summaryResult = await connection.QuerySingleAsync<SummaryResult>(sql, new { from, to });

            // 4. Construa a resposta final a partir dos 4 números que o banco de dados nos deu.
            PaymentSummaryResponse response = PaymentSummaryResponse.Create(
                defaultTotalRequest: (int)summaryResult.DefaultTotalRequest,
                defaultTotalAmount: (float)Math.Round(summaryResult.DefaultTotalAmount, 2),
                fallbackTotalRequest: (int)summaryResult.FallbackTotalRequest,
                fallbackTotalAmount: (float)Math.Round(summaryResult.FallbackTotalAmount, 2)
            );
            //var allPayments = await _context.Payments.ToListAsync();

            //IEnumerable<Payment> filteredPayments = allPayments;
            //if (from.HasValue)
            //{
            //    filteredPayments = filteredPayments.Where(p => p.CreatedAt >= from.Value);
            //}
            //if (to.HasValue)
            //{
            //    filteredPayments = filteredPayments.Where(p => p.CreatedAt <= to.Value);
            //}

            //var finalPaymentsList = filteredPayments.ToList();

            //var response = PaymentSummaryResponse.Create(
            //    defaultTotalRequest: finalPaymentsList.Count(p => !p.IsFallback),
            //    defaultTotalAmount: (float)Math.Round(finalPaymentsList.Where(p => !p.IsFallback).Sum(p => p.Amount), 2),
            //    fallbackTotalRequest: finalPaymentsList.Count(p => p.IsFallback),
            //    fallbackTotalAmount: (float)Math.Round(finalPaymentsList.Where(p => p.IsFallback).Sum(p => p.Amount), 2)
            //);

            return response;
        }

    }
    public record SummaryResult(long DefaultTotalRequest,decimal DefaultTotalAmount,long FallbackTotalRequest,decimal FallbackTotalAmount);
}
