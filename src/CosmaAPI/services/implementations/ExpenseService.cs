using CosmaAPI.data;
using CosmaAPI.DTOs.expenses;
using CosmaAPI.entities;
using CosmaAPI.services.interfaces;
using CosmaAPI.DTOs.common;
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

    public async Task<PagedResponseDTO<ExpenseResponseDTO>> GetAllAsync(
        Guid userId,
        ExpenseQueryDTO query,
        CancellationToken cancellationToken = default)
    {
        if (query.Month.HasValue && !query.Year.HasValue)
        {
            throw new InvalidOperationException("Year is required when filtering by month.");
        }

        if (query.StartDate.HasValue && query.EndDate.HasValue && query.StartDate > query.EndDate)
        {
            throw new InvalidOperationException("StartDate cannot be greater than EndDate.");
        }

        if (query.MinAmount.HasValue && query.MaxAmount.HasValue && query.MinAmount > query.MaxAmount)
        {
            throw new InvalidOperationException("MinAmount cannot be greater than MaxAmount.");
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 100);

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
            var end = start.AddYears(1);

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

        var sortBy = query.SortBy.Trim().ToLowerInvariant();
        var sortDirection = query.SortDirection.Trim().ToLowerInvariant();

        expensesQuery = (sortBy, sortDirection) switch
        {
            ("amount", "asc") => expensesQuery.OrderBy(x => x.Amount),
            ("amount", "desc") => expensesQuery.OrderByDescending(x => x.Amount),

            ("createdat", "asc") => expensesQuery.OrderBy(x => x.CreatedAtUtc),
            ("createdat", "desc") => expensesQuery.OrderByDescending(x => x.CreatedAtUtc),

            ("date", "asc") => expensesQuery.OrderBy(x => x.Date),
            ("date", "desc") => expensesQuery.OrderByDescending(x => x.Date),

            _ => expensesQuery.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAtUtc)
        };

        var totalCount = await expensesQuery.CountAsync(cancellationToken);

        var items = await expensesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MaptoResponseDTO())
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponseDTO<ExpenseResponseDTO>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
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