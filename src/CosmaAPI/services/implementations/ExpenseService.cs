using CosmaAPI.data;
using CosmaAPI.DTOs.expenses;
using CosmaAPI.entities;
using CosmaAPI.services.interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace CosmaAPI.services.implementations;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _dbContext;

    public ExpenseService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseResponseDTO> CreateAsync(
        Guid userId, 
        CreateExpenseRequestDTO request, 
        CancellationToken cancellationToken = default
        )
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);
        if (!categoryExists)        {
            throw new InvalidOperationException("Categoria no encontrada");
        }

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description,
            IsEssential = request.IsEssential,
            PaymentMethod = request.PaymentMethod,
            CategoryId = request.CategoryId,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Expenses.Add(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.Id == expense.Id && x.UserId == userId)
            .Select(MaptoResponseDTO())
            .FirstAsync(cancellationToken);
    }

    public async Task<List<ExpenseResponseDTO>> GetAllAsync(
        Guid userId, 
        ExpenseQueryDTO query, 
        CancellationToken cancellationToken = default
        )
    {
        var expensesQuery = _dbContext.Expenses
        .AsNoTracking()
        .Where(x => x.UserId == userId);

        if (query.Year.HasValue && query.Month.HasValue)
        {
            var start = new DateOnly(query.Year.Value, query.Month.Value, 1);
            var end = start.AddMonths(1);
            expensesQuery = expensesQuery
                .Where(x => x.Date >= start && x.Date < end);
        }
        else if (query.Year.HasValue)
        {
            var start = new DateOnly(query.Year.Value, 1, 1);
            var end = start.AddMonths(1);
            expensesQuery = expensesQuery
                .Where(x => x.Date >= start && x.Date < end);
        }
        if (query.StartDate.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.Date >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.Date <= query.EndDate.Value);
        }
        if (query.CategoryId.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.CategoryId == query.CategoryId.Value);
        }
        if (query.PaymentMethod.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.PaymentMethod == query.PaymentMethod.Value);
        }
        if (query.IsEssential.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.IsEssential == query.IsEssential.Value);
        }
        if (query.MinAmount.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.Amount >= query.MinAmount.Value);
        }
        if (query.MaxAmount.HasValue)
        {
            expensesQuery = expensesQuery
                .Where(x => x.Amount <= query.MaxAmount.Value);
        }

        return await expensesQuery
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(MaptoResponseDTO())
            .ToListAsync(cancellationToken);
    }

    public async Task<ExpenseResponseDTO?> GetByIdAsync(
        Guid userId, 
        Guid expenseId, 
        CancellationToken cancellationToken = default
        )
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.Id == expenseId && x.UserId == userId)
            .Select(MaptoResponseDTO())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExpenseResponseDTO?> UpdateAsync(
        Guid userId, 
        Guid expenseId,
        UpdateExpenseRequestDTO request, 
        CancellationToken cancellationToken = default
        )
    {
        var CategoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);
        if (!CategoryExists)
        {
            throw new InvalidOperationException("Categoria no encontrada");
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId && x.UserId == userId, cancellationToken);
        if (expense == null)
        {
            return null;
        }

        expense.Amount = request.Amount;
        expense.Date = request.Date;
        expense.Description = request.Description;
        expense.IsEssential = request.IsEssential;
        expense.PaymentMethod = request.PaymentMethod;
        expense.CategoryId = request.CategoryId;
        expense.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return await _dbContext.Expenses
            .AsNoTracking()
            .Where(x => x.Id == expense.Id && x.UserId == userId)
            .Select(MaptoResponseDTO())
            .FirstAsync(cancellationToken);

    }

    public async Task<bool> DeleteAsync(
        Guid userId, 
        Guid expenseId, 
        CancellationToken cancellationToken = default
        )
    {
        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId && x.UserId == userId, cancellationToken);
        if (expense == null)
        {
            return false;
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Expression<Func<Expense, ExpenseResponseDTO>> MaptoResponseDTO()
    {
        return expense => new ExpenseResponseDTO
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Date = expense.Date,
            Description = expense.Description,
            IsEssential = expense.IsEssential,
            PaymentMethod = expense.PaymentMethod,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            CreatedAtUtc = expense.CreatedAtUtc,
            UpdatedAtUtc = expense.UpdatedAtUtc
        };
    }
}