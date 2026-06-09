using Microsoft.EntityFrameworkCore;
using Kdbapp.Data;
using Kdbapp.Models;
using BCrypt.Net; 

namespace Kdbapp.Services;

public class AuthService
{
    private readonly KbrddbappContext _db;
    private readonly TokenService _tokenService;
    public AuthService(KbrddbappContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }
    public async Task<string> RegisterUserAsync(RegisterDto model, CancellationToken cancellationToken = default)
    {
        
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Login == model.Login || u.Email == model.Email, cancellationToken);

        if (existingUser != null)
        {
            return existingUser.Email == model.Email 
                ? "Пользователь с такой почтой уже существует" 
                : "Пользователь с таким логином уже существует";
        }

        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
            return "Пароль слишком короткий (минимум 8 символов)";

        var newUser = new User
        {
            Login = model.Login.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), 
            Name = model.Name,
            Surname = model.Surname,
            Email = model.Email,
            PhoneNumber = model.Phone,
            RegisteredAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Role = "user"
        };

        _db.Users.Add(newUser);
    
        await _db.SaveChangesAsync(cancellationToken);
    
        return "Success";
    }
    public async Task<(string? Token, string? ErrorMessage)> LoginUserAsync(LoginDto model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == model.login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.password, user.PasswordHash))
        {
            return (null, "Неверный логин или пароль");
        }
        if (user.Isactive == false)
            return (null, "Аккаунт заблокирован");
        string token = _tokenService.GenerateToken(user);
        return (token, null);
    }
}