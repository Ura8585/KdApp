using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] 
public class UserController : ControllerBase
{
    private readonly KbrddbappContext _db;
    private readonly IWebHostEnvironment _env;

    public UserController(KbrddbappContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized(); 
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });
        return Ok(new
        {
            user.Id,
            user.Login,
            user.Email,
            user.Name,
            user.Surname,
            avatarUrl = user.AvatarUrl 
        });
    }
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Файл не выбран или пуст" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Неверный формат файла. Разрешены только изображения." });
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        var uniqueFileName = $"avatar_{userId}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        user.AvatarUrl = $"/uploads/{uniqueFileName}"; 
        
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return Ok(new { status = "success", avatarUrl = user.AvatarUrl, message = "Аватарка успешно обновлена!" });
    }
}