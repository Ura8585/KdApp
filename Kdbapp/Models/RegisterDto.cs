using System.ComponentModel.DataAnnotations;

namespace Kdbapp.Models;

public class RegisterDto
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Surname { get; set; } = string.Empty;
    [Phone]
    public string Phone { get; set; } = string.Empty;
}