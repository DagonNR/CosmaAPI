using CosmaAPI.data;
using CosmaAPI.DTOs.categories;
using CosmaAPI.services.interfaces;
using Microsoft.EntityFrameworkCore;
namespace CosmaAPI.services.implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CategoryResponseDTO>> GetAllActiveSync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Categories
        .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryResponseDTO
            {
                Id = x.Id,
                Name = x.Name,
                ColorHex = x.ColorHex,
                Icon = x.Icon
            })
            .ToListAsync(cancellationToken);
    }
}