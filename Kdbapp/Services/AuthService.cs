using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Kdbapp.Models;
using Kdbapp.Data;

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
        public async Task<string> RegisterUserAsync(RegisterDto model, CancellationToken cancellationToken =  default)
        {
            var existingUsers = await _db.Users.FirstOrDefaultAsync(u => u.Login == model.Login || u.Email == model.Email);
            if (existingUsers != null)
            {
                if(existingUsers.Email == model.Email)
                {
                    return "Пользователь с такой почтой уже существет";
                }
                return "Пользователь с таким логином или почтой уже существет";
            }
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length <8)
            {
                return "Пароль слишком короткий";
            }
            var newUser = new User{
            Login = model.Login.Trim(),
            PasswordHash = model.Password,
            Name = model.Name,
            Surname = model.Surname,
            Email = model.Email,
            PhoneNumber = model.Phone,
            RegisteredAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Role = "user",
            Isactive =  true
            };
            _db.Users.Add(newUser);                         
            await _db.SaveChangesAsync(cancellationToken);      
            return "Success";
        }
        public async Task<(string? Token, string? ErrorMessage)> LoginUserAsync(LoginDto model)       
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Login == model.login);
            if (user == null || user.PasswordHash != model.password)
                return (null, "Неверный логин или пароль");
            if (user.Isactive == false)
                return (null, "Аккаунт заблокирован. Обратитесь к администратору.");
            string? token = _tokenService.GenerateToken(user);
            return (token, null);
        }
    } 
 