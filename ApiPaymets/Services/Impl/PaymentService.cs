using ApiPaymets.Database;
using ApiPaymets.Database.Entities;
using ApiPaymets.RequisitonsModels.Responses;
using Dapper;
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
                await using var connection = _context.Database.GetDbConnection();
                var sql = @"
                    INSERT INTO ""Payments"" (""CorrelationId"", ""Amount"", ""CreatedAt"", ""IsFallback"")
                    VALUES (@CorrelationId, @Amount, @CreatedAt, @IsFallback);
                ";

                // Dapper executa o INSERT de forma segura, usando parâmetros.
                var rowsAffected = await connection.ExecuteAsync(sql, payment);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar pagamento {CorrelationId} com Dapper.", payment.CorrelationId);
                return false;
            }
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
                WHERE (""CreatedAt"" >= @from OR @from IS NULL)
                AND (""CreatedAt"" <= @to OR @to IS NULL);
            ";

            var summaryResult = await connection.QuerySingleAsync<SummaryResult>(sql, new { from, to });

            // 4. Construa a resposta final a partir dos 4 números que o banco de dados nos deu.
            PaymentSummaryResponse response = PaymentSummaryResponse.Create(
                defaultTotalRequest: (int)summaryResult.DefaultTotalRequest,
                defaultTotalAmount: Math.Round(summaryResult.DefaultTotalAmount, 2),
                fallbackTotalRequest: (int)summaryResult.FallbackTotalRequest,
                fallbackTotalAmount: Math.Round(summaryResult.FallbackTotalAmount, 2)
            );

            return response;
        }

    }
    public record SummaryResult(long DefaultTotalRequest,decimal DefaultTotalAmount,long FallbackTotalRequest,decimal FallbackTotalAmount);
}
