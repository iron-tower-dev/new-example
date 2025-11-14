using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;

namespace LabResultsApi.Services;

public class TransactionService : ITransactionService
{
    private readonly LabDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(LabDbContext context, ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogDebug("Starting database transaction");
            
            var result = await operation();
            
            await transaction.CommitAsync();
            _logger.LogDebug("Database transaction committed successfully");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database transaction, rolling back");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogDebug("Starting database transaction");
            
            await operation();
            
            await transaction.CommitAsync();
            _logger.LogDebug("Database transaction committed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database transaction, rolling back");
            await transaction.RollbackAsync();
            throw;
        }
    }
}