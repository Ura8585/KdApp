using System.Security.Claims;
using Kdbapp.Models;
using Microsoft.AspNetCore.Mvc;
using Kdbapp.Services;

namespace Kdbapp.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationService _configService;
    public ConfigurationController(ConfigurationService configService)
    {
        _configService = configService;
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
}