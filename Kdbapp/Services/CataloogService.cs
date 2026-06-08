using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;
using Kdbapp.Models;

namespace Kdbapp.Services;

public class CatalogService
{
    private readonly KbrddbappContext _db;
    public CatalogService(KbrddbappContext db)
    {
        _db = db;
    }
    public async Task<CatalogDto> GetCatalogDataAsync()
    {
        var allAvailableItems = await _db.Components
            .Where(c => c.InStook == true) 
            .ToListAsync();
        return new CatalogDto
        {
            Cases = allAvailableItems.Where(c => c.Category == "case").ToList(),
            Switches = allAvailableItems.Where(c => c.Category == "switch").ToList(),
            Keycaps = allAvailableItems.Where(c => c.Category == "keycap").ToList()
        };
    }
}