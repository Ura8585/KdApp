using Microsoft.AspNetCore.Mvc;
using Kdbapp.Services;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }
    [HttpGet]
    public async Task<IActionResult> GetCatalog()
    {
        var data = await _catalogService.GetCatalogDataAsync();
        return Ok(data); 
    }
}