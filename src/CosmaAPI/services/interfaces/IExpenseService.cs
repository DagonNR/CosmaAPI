using CosmaAPI.DTOs.expenses;
namespace CosmaAPI.services.interfaces;

public interface IExpenseService
{
    Task<ExpenseResponseDTO> CreateAsync(
        Guid userId,
        CreateExpenseRequestDTO request,
        CancellationToken cancellationToken = default
        );
    Task<List<ExpenseResponseDTO>> GetAllAsync(
        Guid userId,
        ExpenseQueryDTO query,
        CancellationToken cancellationToken = default
        );
    Task<ExpenseResponseDTO?> GetByIdAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default
        );
    Task<ExpenseResponseDTO?> UpdateAsync(
        Guid userId,
        Guid expenseId,
        UpdateExpenseRequestDTO request,
        CancellationToken cancellationToken = default
        );
    Task<bool> DeleteAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default
        );
}