using CosmaAPI.DTOs.categories;
using CosmaAPI.services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CosmaAPI.controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDTO>>> GetAll(
        CancellationToken cancellationToken = default
    )
    {
        var categories = await _categoryService.GetAllActiveSync(cancellationToken);
        return Ok(categories);
    }
}