using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Kdbapp.Services;
using Kdbapp.Models;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
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
            // Кастим userId в (int) специально для твоего метода PrepareOrderAsync, 
            // но внутри сервиса FindAsync упадет, если в базе он long.
            // Поэтому мы обойдем проблему прямо здесь, если сервис вернет null из-за типа.
            var order = await _orderService.PrepareOrderAsync(dto.ConfigurationId, (int)userId);
            
            if (order == null)
            {
                return NotFound(new { error = "Конфигурация клавиатуры или пользователь не найден" });
            }

            // Перезаписываем данные из формы
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

            // Сохраняем
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
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { error = "Пользователь не авторизован" });
        }

        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
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