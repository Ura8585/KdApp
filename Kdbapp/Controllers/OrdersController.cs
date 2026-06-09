using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Kdbapp.Services;
using Kdbapp.Models;
using Kdbapp.Data;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly KbrddbappContext _db;
    [HttpPut("{id}/change-configuration")]
public async Task<IActionResult> ChangeConfiguration(long id, [FromBody] ChangeConfigurationDto dto)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
    {
        return Unauthorized(new { error = "Пользователь не авторизован" });
    }

    var order = await _db.Orders
        .Include(o => o.Configuration)
        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

    if (order == null)
        return NotFound(new { error = "Заказ не найден" });

    // Только статус 1 (Новый)
    if (order.Status != 1)
        return BadRequest(new { error = "Можно менять конфигурацию только в статусе 'Новый'" });

    var config = order.Configuration;
    if (config == null)
        return NotFound(new { error = "Конфигурация не найдена" });

    // Обновляем компоненты
    if (dto.CaseId.HasValue) config.CasesizeId = dto.CaseId.Value;
    if (dto.SwitchId.HasValue) config.SwitchtypeId = dto.SwitchId.Value;
    if (dto.KeycapId.HasValue) config.KeycapsId = dto.KeycapId.Value;

    // Пересчитываем цену
    var caseComponent = dto.CaseId.HasValue ? await _db.Components.FindAsync(dto.CaseId.Value) : null;
    var switchComponent = dto.SwitchId.HasValue ? await _db.Components.FindAsync(dto.SwitchId.Value) : null;
    var keycapComponent = dto.KeycapId.HasValue ? await _db.Components.FindAsync(dto.KeycapId.Value) : null;

    order.TotalPrice = (caseComponent?.Price ?? config.Casesize?.Price ?? 0) +
                       (switchComponent?.Price ?? 0) +
                       (keycapComponent?.Price ?? config.Keycaps?.Price ?? 0);

    await _db.SaveChangesAsync();

    return Ok(new { 
        message = "Конфигурация обновлена", 
        orderId = order.Id,
        totalPrice = order.TotalPrice 
    });
}

// Добавь DTO класс в конец файла
public class ChangeConfigurationDto
{
    public int? CaseId { get; set; }
    public int? SwitchId { get; set; }
    public int? KeycapId { get; set; }
}

    public OrdersController(OrderService orderService, KbrddbappContext db)
    {
        _orderService = orderService;
        _db = db;
    }

    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(new { error = "Пользователь не авторизован или токен недействителен" });
        }

        try
        {
            var order = await _orderService.PrepareOrderAsync(dto.ConfigurationId, userId);
            
            if (order == null)
            {
                return NotFound(new { error = "Конфигурация клавиатуры или пользователь не найден" });
            }

            order.ShippingAddress = dto.ShippingAddress;
            if (!string.IsNullOrWhiteSpace(dto.ContactEmail))
            {
                order.Contactemail = dto.ContactEmail;
            }
            if (dto.Quantity > 0)
            {
                order.Quantity = dto.Quantity;
                order.TotalPrice = order.TotalPrice * dto.Quantity;
            }

            long orderId = await _orderService.PlaceOrderAsync(order);

            return Ok(new { orderId = orderId, message = "Заказ успешно оформлен!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Внутренняя ошибка сервера: {ex.Message}" });
        }
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(new { error = "Пользователь не авторизован" });
        }

        var orders = await _orderService.GetUserOrdersAsync(userId);
        
        // Добавляем название статуса
        var result = orders.Select(o => new
        {
            o.Id,
            o.ConfigurationId,
            o.TotalPrice,
            o.Quantity,
            Status = o.StatusNavigation?.Name ?? "Неизвестно",
            o.CreatedAt,
            o.ShippingAddress,
            o.Contactemail
        });

        return Ok(result);
    }

    [HttpPut("{id}/update")]
    public async Task<IActionResult> UpdateOrder(long id, [FromBody] UpdateOrderDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(new { error = "Пользователь не авторизован" });
        }

        var order = await _db.Orders
            .Include(o => o.StatusNavigation)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { error = "Заказ не найден" });
        }

        // Проверяем статус (1 = Новый, 2 = В обработке)
        if (order.Status != 1 && order.Status != 2)
        {
            return BadRequest(new { error = "Можно изменять только заказы в статусе 'Новый' или 'В обработке'" });
        }

        if (!string.IsNullOrWhiteSpace(dto.ShippingAddress))
        {
            if (dto.ShippingAddress.Length < 8)
            {
                return BadRequest(new { error = "Адрес должен быть минимум 8 символов" });
            }
            order.ShippingAddress = dto.ShippingAddress;
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Заказ обновлён", orderId = order.Id });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(long id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            return Unauthorized(new { error = "Пользователь не авторизован" });
        }

        var order = await _db.Orders
            .Include(o => o.StatusNavigation)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { error = "Заказ не найден" });
        }

        // Проверяем статус (1 = Новый, 2 = В обработке)
        if (order.Status != 1 && order.Status != 2)
        {
            return BadRequest(new { error = "Можно отменить только заказы в статусе 'Новый' или 'В обработке'" });
        }

        // Статус "Отменён" = 6 (если у тебя такой id)
        order.Status = 5;

        // Записываем в историю
        var history = new Orderstatushistory
        {
            OrderId = order.Id,
            Status = 6,
            Comment = "Заказ отменён пользователем",
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        _db.Orderstatushistories.Add(history);

        await _db.SaveChangesAsync();

        return Ok(new { message = "Заказ отменён", orderId = order.Id });
    }
}


public class PlaceOrderDto
{
    public long ConfigurationId { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class UpdateOrderDto
{
    public string? ShippingAddress { get; set; }
}