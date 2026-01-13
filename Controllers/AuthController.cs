using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;

namespace TicketManagementSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthController(AuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = _authService.Login(request.Email, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!user.IsLoginAllowed)
                {
                    return BadRequest(new { message = "Login not allowed for this user" });
                }

                return Ok(new { 
                    user = new 
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.Role,
                        user.IsLoginAllowed
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var success = _authService.Register(request.Username, request.Password, request.Email);
                if (!success)
                {
                    return BadRequest(new { message = "Registration failed - username or email already exists" });
                }

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
