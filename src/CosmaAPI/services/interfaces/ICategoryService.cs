using CosmaAPI.DTOs.categories;
namespace CosmaAPI.services.interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponseDTO>> GetAllActiveSync(
        CancellationToken cancellationToken = default
    );
}