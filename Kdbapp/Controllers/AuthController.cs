using Microsoft.AspNetCore.Mvc;
using Kdbapp.Services;
using Kdbapp.Models;

namespace Kdbapp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterUserAsync(model,cancellationToken);
        if (result == "Success")
        { return Ok(new{message = "success register user"}); }
        return BadRequest(new {error = result});
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var (token, errorMessage) = await _authService.LoginUserAsync(model);
        if (errorMessage != null)
        {
            return BadRequest(new{error = errorMessage});
        }
        return Ok(new{token = token});
    }
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("ping");
    }
}