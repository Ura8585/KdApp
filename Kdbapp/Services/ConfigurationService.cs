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
            .Include(k => k.Switchtype)
            .Include(k => k.Keycaps)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (config == null) return null;

        return new ConfigurationDetailsDto
        {
            ConfigurationId = config.Id,
            Case = config.Casesize,
            Switch = config.Switchtype,
            Keycap = config.Keycaps,
            TotalPrice = (config.Casesize?.Price ?? 0) + (config.Switchtype?.Price ?? 0) + (config.Keycaps?.Price ?? 0)
        };
    }
}