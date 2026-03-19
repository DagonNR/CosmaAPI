using System.Security.Claims;
using CosmaAPI.DTOs.expenses;
using CosmaAPI.services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CosmaAPI.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDTO>> Create(
        [FromBody] CreateExpenseRequestDTO request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _expenseService.CreateAsync(
                userId,
                request,
                cancellationToken
            );
            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseResponseDTO>>> GetAll(
        [FromQuery] ExpenseQueryDTO query,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        var expenses = await _expenseService.GetAllAsync(
            userId,
            query,
            cancellationToken
        );
        return Ok(expenses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDTO>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        var expense = await _expenseService.GetByIdAsync(
            userId,
            id,
            cancellationToken
        );
        if (expense == null)
        {
            return NotFound(new { message = "Gasto no encontrado." });
        }
        return Ok(expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDTO>> Update(
        Guid id,
        [FromBody] UpdateExpenseRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var expense = await _expenseService.UpdateAsync(
                userId,
                id,
                request,
                cancellationToken
            );
            if (expense == null)
            {
                return NotFound(new { message = "Gasto no encontrado." });
            }
            return Ok(expense);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var userId = GetCurrentUserId();
        var deleted = await _expenseService.DeleteAsync(
            userId,
            id,
            cancellationToken
        );
        if (!deleted)
        {
            return NotFound(new { message = "Gasto no encontrado." });
        }
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token invalido.");
        }
        return userId;
    }
}