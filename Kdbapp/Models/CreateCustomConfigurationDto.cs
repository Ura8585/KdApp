using System.ComponentModel.DataAnnotations;

namespace Kdbapp.Models;

public class CreateCustomConfigurationDto
{
    public string Layout { get; set; } = "80"; 
    public string CaseColor { get; set; } = "#22d3ee";
    public string KeycapColor { get; set; } = "#111111";
    public string VolumeColor { get; set; } = "#111111";
    public string SwitchColor { get; set; } = "#ef4444";

    public string KeycapMaterialType { get; set; } = "matte"; 
    public string SwitchType { get; set; } = "linear";       

    public bool RgbMode { get; set; } = false;
    public bool HasCustomPrint { get; set; } = false;

    public string TotalPrice { get; set; } = "8500";
    
    public IFormFile? PrintImage { get; set; }
}