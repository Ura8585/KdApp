using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;
using Kdbapp.Models;

namespace Kdbapp.Services;

public class ConfigurationService
{
    private readonly KbrddbappContext _db;

    public ConfigurationService(KbrddbappContext db)
    {
        _db = db;
    }

    public async Task<long> CreateConfigurationAsync(int caseId, int switchId, int keycapId, int? userId = null)
    {
        var newConfig = new Keyboardconfiguration
        {
            CasesizeId = caseId,
            SwitchtypeId = switchId,
            KeycapsId = keycapId,
            UserId = userId,
            Configurationhash = Guid.NewGuid().ToString()
        };

        _db.Keyboardconfigurations.Add(newConfig);
        await _db.SaveChangesAsync();

        return newConfig.Id;
    }

    public async Task<ConfigurationDetailsDto?> GetConfigurationDetailsAsync(long id)
    {
        var config = await _db.Keyboardconfigurations
            .Include(k => k.Casesize)
            .Include(k => k.Keycaps)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (config == null) return null;

        return new ConfigurationDetailsDto
        {
            ConfigurationId = config.Id,
            Case = config.Casesize,
            Keycap = config.Keycaps,
            TotalPrice = (config.Casesize?.Price ?? 0) + (config.Keycaps?.Price ?? 0)
        };
    }

    public async Task<long> CreateCustomConfigurationAsync(CreateCustomConfigurationDto dto, int? userId = null)
    {
        string? printImagePath = null;

        if (dto.PrintImage != null)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "prints");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{dto.PrintImage.FileName}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await dto.PrintImage.CopyToAsync(stream);

            printImagePath = $"/uploads/prints/{fileName}";
        }

        var config = new Keyboardconfiguration
        {
            CasesizeId = 1,
            SwitchtypeId = 1,
            KeycapsId = 1,
            UserId = userId,
            Configurationhash = Guid.NewGuid().ToString(),
            Layout = dto.Layout,
            CaseColor = dto.CaseColor,
            KeycapColor = dto.KeycapColor,
            VolumeColor = dto.VolumeColor,
            SwitchColor = dto.SwitchColor,
            KeycapMaterialType = dto.KeycapMaterialType,
            SwitchType = dto.SwitchType,
            RgbMode = dto.RgbMode,
            HasCustomPrint = dto.HasCustomPrint,
            CustomPrintImageUrl = printImagePath,
            TotalPriceRaw = dto.TotalPrice
        };

        _db.Keyboardconfigurations.Add(config);
        await _db.SaveChangesAsync();

        return config.Id;
    }
}