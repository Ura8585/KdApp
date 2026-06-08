using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;    
using Kdbapp.Models;  

namespace Kdbapp.Services;

public class OrderService
{
    private readonly KbrddbappContext _db;
    public OrderService(KbrddbappContext db)
    {
        _db = db;
    }
    
    public async Task<Order?> PrepareOrderAsync(long configId, long userId)
    {
        var config = await _db.Keyboardconfigurations
            .Include(k => k.Casesize)
            .Include(k => k.Switchtype) 
            .Include(k => k.Keycaps)   
            .FirstOrDefaultAsync(k => k.Id == configId);
        if (config == null) return null;
        var user = await _db.Users.FindAsync(userId); 
        if (user == null) return null;
        if (config.UserId == 0 || config.UserId == null)
        {
            config.UserId = user.Id;
            _db.Keyboardconfigurations.Update(config);
            await _db.SaveChangesAsync();
        }
        return new Order
        {
            ConfigurationId = config.Id,
            UserId = user.Id, 
            Contactemail = user.Email,
            TotalPrice = CalculateTotal(config),
            Quantity = 1
        };
    }
    public async Task<long> PlaceOrderAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.ShippingAddress) || order.ShippingAddress.Length < 8)
        {
            throw new ArgumentException("Укажите корректный адрес доставки (минимум 8 символов)");
        }
        order.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        order.Status = 1; 

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(); 

        var history = new Orderstatushistory
        {
            OrderId = order.Id,
            Status = 1, 
            Comment = "Заказ успешно создан пользователем",
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        _db.Orderstatushistories.Add(history);
        await _db.SaveChangesAsync();

        return order.Id; 
    }
    
    public async Task<List<Order>> GetUserOrdersAsync(long userId)
    {
        return await _db.Orders
            .Include(o => o.StatusNavigation) 
            .Include(o => o.Configuration)
                .ThenInclude(c => c.Casesize)
            .Include(o => o.Configuration)
                .ThenInclude(c => c.Switchtype)
            .Include(o => o.Configuration)
                .ThenInclude(c => c.Keycaps)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt) 
            .ToListAsync();
    }

    private decimal CalculateTotal(Keyboardconfiguration config)
    {
        return (config.Casesize?.Price ?? 0) +
               (config.Switchtype?.Price ?? 0) +
               (config.Keycaps?.Price ?? 0);
    }
}