using Kdbapp.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;
using Kdbapp.Models;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly KbrddbappContext _db;

    public AdminController(KbrddbappContext db)
    {
        _db = db;
    }

    // Проверка доступа прямо в методах
    private bool IsAdmin()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"DEBUG AUTH: role={role}, userId={nameId}");
        return role == "admin" || role == "moderator";
    }

    private bool IsAdminOnly()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return role == "admin";
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        if (!IsAdmin()) return Forbid();

        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.StatusNavigation)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new
        {
            o.Id,
            UserLogin = o.User?.Login ?? "—",
            o.TotalPrice,
            o.Quantity,
            Status = o.StatusNavigation?.Name ?? "—",
            o.CreatedAt,
            o.ShippingAddress,
            o.Contactemail
        });

        return Ok(result);
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateStatusDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = dto.StatusId;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Статус обновлён" });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        if (!IsAdminOnly()) return Forbid();

        var users = await _db.Users.ToListAsync();
        var result = users.Select(u => new
        {
            u.Id,
            u.Login,
            u.Email,
            u.Role,
            u.Isactive,
            u.RegisteredAt
        });
        return Ok(result);
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(long id, [FromBody] UpdateRoleDto dto)
    {
        if (!IsAdminOnly()) return Forbid();

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Role = dto.Role;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Роль изменена на {dto.Role}" });
    }

    [HttpPut("users/{id}/ban")]
    public async Task<IActionResult> ToggleBan(long id)
    {
        if (!IsAdminOnly()) return Forbid();

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Isactive = !(user.Isactive ?? true);
        await _db.SaveChangesAsync();
        return Ok(new { message = user.Isactive == true ? "Пользователь разблокирован" : "Пользователь заблокирован" });
    }

    [HttpGet("gallery/all")]
    public async Task<IActionResult> GetAllGallery()
    {
        if (!IsAdmin()) return Forbid();

        var all = await _db.Galleries
            .Include(g => g.Author)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var result = all.Select(g => new
        {
            g.Id,
            g.Title,
            Author = g.Author?.Login ?? "Аноним",
            g.Authorid,
            g.ConfigId,
            g.IsModerated,
            g.CreatedAt,
            g.Likescount
        });

        return Ok(result);
    }

    [HttpDelete("gallery/{id}")]
    public async Task<IActionResult> DeleteGalleryItem(long id)
    {
        if (!IsAdmin()) return Forbid();

        var item = await _db.Galleries.FindAsync(id);
        if (item == null) return NotFound();
        _db.Galleries.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Удалено из галереи" });
    }

    [HttpPut("gallery/{id}/moderate")]
    public async Task<IActionResult> ModerateGallery(long id, [FromBody] ModerateDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null) return NotFound();
        gallery.IsModerated = dto.Approved;
        await _db.SaveChangesAsync();
        return Ok(new { message = dto.Approved ? "Одобрено" : "Снято с публикации" });
    }

    [HttpPost("components")]
    public async Task<IActionResult> AddComponent([FromBody] AddComponentDto dto)
    {
        if (!IsAdmin()) return Forbid();

        var component = new Component
        {
            Category = dto.Category,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            InStook = dto.InStook,
            ImageUrl = dto.ImageUrl
        };
        _db.Components.Add(component);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Компонент добавлен", componentId = component.Id });
    }

    [HttpDelete("components/{id}")]
    public async Task<IActionResult> DeleteComponent(int id)
    {
        if (!IsAdmin()) return Forbid();

        var component = await _db.Components.FindAsync(id);
        if (component == null) return NotFound();
        _db.Components.Remove(component);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Компонент удалён" });
    }

    [HttpGet("components")]
    public async Task<IActionResult> GetAllComponents()
    {
        if (!IsAdmin()) return Forbid();

        var components = await _db.Components.OrderBy(c => c.Category).ThenBy(c => c.Name).ToListAsync();
        return Ok(components);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        if (!IsAdmin()) return Forbid();

        var totalOrders = await _db.Orders.CountAsync();
        var totalUsers = await _db.Users.CountAsync();
        var totalRevenue = await _db.Orders.SumAsync(o => o.TotalPrice);
        var totalGallery = await _db.Galleries.CountAsync();
        var totalComponents = await _db.Components.CountAsync();

        return Ok(new { totalOrders, totalUsers, totalRevenue, totalGallery, totalComponents });
    }
    [HttpGet("assembler/orders")]
    [RoleAuthorize("admin", "moderator", "assembler")]
    public async Task<IActionResult> GetAssemblerOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Configuration)
                .ThenInclude(c => c.Casesize)
            .Include(o => o.Configuration)
                .ThenInclude(c => c.Keycaps)
            .Include(o => o.StatusNavigation)
            .Where(o => o.Status == 2 || o.Status == 6)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new
        {
            o.Id,
            UserLogin = o.User?.Login ?? "—",
            o.TotalPrice,
            Status = o.StatusNavigation?.Name ?? "—",
            StatusId = o.Status,
            o.CreatedAt,
            o.ShippingAddress,
            ConfigId = o.Configuration?.Id,
            CaseName = o.Configuration?.Casesize?.Name,
            KeycapName = o.Configuration?.Keycaps?.Name,
            Layout = o.Configuration?.Layout,
            CaseColor = o.Configuration?.CaseColor,
            KeycapColor = o.Configuration?.KeycapColor,
            SwitchType = o.Configuration?.SwitchType,
            SwitchColor = o.Configuration?.SwitchColor,
            KeycapMaterialType = o.Configuration?.KeycapMaterialType,
            HasCustomPrint = o.Configuration?.HasCustomPrint
        });

        return Ok(result);
    }

    [HttpPut("assembler/orders/{id}/start")]
    [RoleAuthorize("admin", "moderator", "assembler")]
    public async Task<IActionResult> StartAssembly(long id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = 6;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Сборка начата", orderId = id });
    }

    [HttpPut("assembler/orders/{id}/complete")]
    [RoleAuthorize("admin", "moderator", "assembler")]
    public async Task<IActionResult> CompleteAssembly(long id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = 3;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Сборка завершена!", orderId = id });
    }
}

public class UpdateStatusDto { public int StatusId { get; set; } }
public class UpdateRoleDto { public string Role { get; set; } = "user"; }
public class ModerateDto { public bool Approved { get; set; } }
public class AddComponentDto
{
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool InStook { get; set; } = true;
    public string? ImageUrl { get; set; }
}