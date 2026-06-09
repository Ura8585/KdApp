using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;
using Kdbapp.Models;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GalleryController : ControllerBase
{
    private readonly KbrddbappContext _db;

    public GalleryController(KbrddbappContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var gallery = await _db.Galleries
            .Include(g => g.Config)
                .ThenInclude(c => c.Casesize)
            .Include(g => g.Config)
                .ThenInclude(c => c.Keycaps)
            .Include(g => g.Author)
            .Where(g => g.IsModerated == true)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var result = new List<object>();

        foreach (var g in gallery)
        {
            var config = g.Config;
            result.Add(new
            {
                id = g.Id,
                title = g.Title,
                author = g.Author?.Login ?? "Аноним",
                authorAvatar = g.Author?.AvatarUrl,
                configId = g.ConfigId,
                layout = config?.Layout,
                caseColor = config?.CaseColor,
                keycapColor = config?.KeycapColor,
                volumeColor = config?.VolumeColor,
                switchColor = config?.SwitchColor,
                keycapMaterialType = config?.KeycapMaterialType,
                switchType = config?.SwitchType,
                rgbMode = config?.RgbMode,
                hasCustomPrint = config?.HasCustomPrint,
                customPrintImageUrl = config?.CustomPrintImageUrl,
                caseName = config?.Casesize?.Name,
                keycapName = config?.Keycaps?.Name,
                totalPrice = (config?.Casesize?.Price ?? 0) + (config?.Keycaps?.Price ?? 0),
                createdAt = g.CreatedAt,
                likesCount = g.Likescount
            });
        }

        return Ok(result);
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] PublishToGalleryDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            return Unauthorized(new { error = "Не авторизован" });

        var config = await _db.Keyboardconfigurations.FindAsync(dto.ConfigId);
        if (config == null)
            return NotFound(new { error = "Конфигурация не найдена" });

        var existing = await _db.Galleries.FirstOrDefaultAsync(g => g.ConfigId == dto.ConfigId);
        if (existing != null)
            return BadRequest(new { error = "Эта конфигурация уже опубликована" });

        var gallery = new Gallery
        {
            ConfigId = dto.ConfigId,
            Authorid = userId,
            Title = dto.Title ?? "Моя сборка",
            IsModerated = true,
            Likescount = true,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        _db.Galleries.Add(gallery);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Опубликовано в галерее!", galleryId = gallery.Id });
    }

    [HttpPost("{id}/like")]
    public async Task<IActionResult> Like(long id)
    {
        var gallery = await _db.Galleries.FindAsync(id);
        if (gallery == null)
            return NotFound();

        gallery.Likescount = true;
        await _db.SaveChangesAsync();

        return Ok(new { likes = 1 });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
            return Unauthorized();

        var gallery = await _db.Galleries.FirstOrDefaultAsync(g => g.Id == id && g.Authorid == userId);
        if (gallery == null)
            return NotFound(new { error = "Публикация не найдена" });

        _db.Galleries.Remove(gallery);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Удалено из галереи" });
    }
}

public class PublishToGalleryDto
{
    public long ConfigId { get; set; }
    public string? Title { get; set; }
}