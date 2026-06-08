using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Kdbapp.Data;
using Kdbapp.Models;
using Kdbapp.Services;
using System.Text.Json.Serialization; 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
        options.AddPolicy("AllowNextJS", policy =>
        {
                policy.WithOrigins("http://localhost:3000") // Адрес твоего фронтенда
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Чтобы куки и токены летали без проблем
        });
});
builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CatalogService>();       
builder.Services.AddScoped<ConfigurationService>(); 
builder.Services.AddScoped<OrderService>();

builder.Services.AddDbContext<KbrddbappContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
var JwtKey =  builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
        options.TokenValidationParameters = new TokenValidationParameters
        {
                ValidateIssuer =  true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))

        };
});
var app = builder.Build();
app.UseCors("AllowNextJS");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
