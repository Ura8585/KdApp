namespace Kdbapp.Models;

public class CatalogDto
{
    public List<Component> Cases { get; set; } = new();
    public List<Component> Switches { get; set; } = new();
    public List<Component> Keycaps { get; set; } = new();
}