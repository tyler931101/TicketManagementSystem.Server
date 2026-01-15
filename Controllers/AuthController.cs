using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;
using TicketManagementSystem.Server.DTOs.Auth;
using TicketManagementSystem.Server.DTOs.Common;
using TicketManagementSystem.Server.DTOs.Users;

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
        public IActionResult Login([FromBody] DTOs.Auth.LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid request data"));
                }

                var user = _authService.LoginAsync(request.Email, request.Password);

                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("Invalid credentials"));
                }

                if (!user.IsLoginAllowed)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Login not allowed for this user"));
                }

                var userDto = new DTOs.Auth.UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    AvatarPath = user.AvatarPath,
                    IsLoginAllowed = user.IsLoginAllowed,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                var authResponse = new DTOs.Auth.AuthResponse
                {
                    User = userDto,
                    Token = "", // TODO: Implement JWT token generation
                    ExpiresAt = DateTime.Now.AddHours(24)
                };

                return Ok(ApiResponse<DTOs.Auth.AuthResponse>.SuccessResult(authResponse, "Login successful"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DTOs.Auth.RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid request data"));
                }

                var success = _authService.RegisterAsync(request.Username, request.Password, request.Email);
                if (!success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Registration failed - username or email already exists"));
                }

                return Ok(ApiResponse<object>.SuccessResult(new { }, "User registered successfully"));
            }
            catch (Exception ex)
            {
                // Handle exceptions from AuthService (database errors, etc.)
                return StatusCode(500, ApiResponse<object>.ErrorResult("Registration failed due to server error", new List<string> { ex.Message }));
            }
        }
    }
}
