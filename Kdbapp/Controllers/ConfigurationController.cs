using System.Security.Claims;
using Kdbapp.Models;
using Kdbapp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kdbapp.Services;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationService _configService;
    private readonly KbrddbappContext _db;

    public ConfigurationController(ConfigurationService configService, KbrddbappContext db)
    {
        _configService = configService;
        _db = db;
    }

    [HttpPost("create-custom")]
    public async Task<IActionResult> CreateCustom([FromForm] CreateCustomConfigurationDto dto)
    {
        try
        {
            int? userId = null;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int parsedId))
                userId = parsedId;

            var configId = await _configService.CreateCustomConfigurationAsync(dto, userId);

            return Ok(new
            {
                status = "success",
                configurationId = configId,
                message = "Кастомная сборка успешно сохранена"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationDto model)
    {
        int? userId = null;
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int parsedId))
        {
            userId = parsedId;
        }
        long configId = await _configService.CreateConfigurationAsync(model.CaseId, model.SwitchId, model.KeycapId, userId);

        return Ok(new { status = "success", configurationId = configId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConfiguration(long id)
    {
        var details = await _configService.GetConfigurationDetailsAsync(id);
        if (details == null)
        {
            return NotFound(new { message = "Конфигурация не найдена" });
        }
        return Ok(details);
    }

    [HttpGet("{id}/full")]
    public async Task<IActionResult> GetFullConfiguration(long id)
    {
        var config = await _db.Keyboardconfigurations
            .Include(k => k.Casesize)
            .Include(k => k.Keycaps)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (config == null)
            return NotFound(new { message = "Конфигурация не найдена" });

        var switchComponent = await _db.Components.FindAsync(config.SwitchtypeId);

        return Ok(new
        {
            configurationId = config.Id,
            layout = config.Layout,
            caseColor = config.CaseColor,
            keycapColor = config.KeycapColor,
            volumeColor = config.VolumeColor,
            switchColor = config.SwitchColor,
            keycapMaterialType = config.KeycapMaterialType,
            switchType = config.SwitchType,
            rgbMode = config.RgbMode,
            hasCustomPrint = config.HasCustomPrint,
            customPrintImageUrl = config.CustomPrintImageUrl,
            totalPriceRaw = config.TotalPriceRaw,
            caseComp = config.Casesize == null ? null : new { id = config.Casesize.Id, name = config.Casesize.Name, price = config.Casesize.Price },
            switchComp = switchComponent == null ? null : new { id = switchComponent.Id, name = switchComponent.Name, price = switchComponent.Price },
            keycap = config.Keycaps == null ? null : new { id = config.Keycaps.Id, name = config.Keycaps.Name, price = config.Keycaps.Price },
            totalPrice = (config.Casesize?.Price ?? 0) + (switchComponent?.Price ?? 0) + (config.Keycaps?.Price ?? 0)
        });
    }
}