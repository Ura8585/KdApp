namespace Kdbapp.Models;

public class ConfigurationDetailsDto
{
    public long ConfigurationId { get; set; }
    public Component? Case { get; set; }
    public Component? Switch { get; set; }
    public Component? Keycap { get; set; }
    public decimal TotalPrice { get; set; }
}