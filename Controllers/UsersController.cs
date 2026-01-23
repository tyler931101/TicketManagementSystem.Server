using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;
using TicketManagementSystem.Server.DTOs.Users;
using TicketManagementSystem.Server.DTOs.Common;

namespace TicketManagementSystem.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(idClaim, out var uid);
            return uid;
        }

        [HttpGet]
        [ResponseCache(Duration = 30)] // Cache for 30 seconds
        public async Task<IActionResult> GetAllUsers([FromQuery] PagedRequestDto request)
        {
            try
            {
                // Validate request
                if (request.PageSize > 100)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Page size cannot exceed 100", 
                        new List<string> { "Maximum page size is 100" }));
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = await _userService.GetAllUsersAsync(request);
                stopwatch.Stop();

                // Add performance metadata
                Response.Headers.Append("X-Response-Time", stopwatch.ElapsedMilliseconds.ToString());
                Response.Headers.Append("X-Total-Records", result.TotalRecords.ToString());

                return Ok(ApiResponse<PagedResponseDto<User>>.SuccessResult(result, "Users retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid request parameters", 
                    new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                // Log the exception for monitoring
                Console.WriteLine($"Error retrieving users: {ex}");
                return StatusCode(500, ApiResponse<object>.ErrorResult("An error occurred while retrieving users", 
                    new List<string> { "Internal server error" }));
            }
        }

        [HttpGet("ticket-users")]
        public async Task<IActionResult> GetTicketUsers()
        {
            try
            {
                var users = await _userService.GetTicketUsersAsync();
                
                if (users == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Unable to retrieve users from service"));
                }
                
                var ticketUsers = users.Select(u => new TicketUserDto
                {
                    Id = u.Id,
                    Username = u.Username
                }).ToList();
                
                return Ok(ApiResponse<List<TicketUserDto>>.SuccessResult(ticketUsers, "Ticket users retrieved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid operation: " + ex.Message));
            }
            catch (Exception ex)
            {
                // Log the exception (in production, you'd use proper logging)
                Console.WriteLine($"Error in GetTicketUsers: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResult("An unexpected error occurred while retrieving ticket users"));
            }
        }

        [HttpPost("{id}/toggle-login")]
        public async Task<IActionResult> ToggleLogin(Guid id, [FromBody] ToggleLoginRequest request)
        {
            try
            {
                var user = new User { Id = id };
                await _userService.SetLoginAllowedAsync(user, request.IsAllowed);
                return Ok(ApiResponse<object>.SuccessResult(new { }, "Login permission updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var uid = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(uid);
                if (user == null) return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                var dto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role,
                    AvatarPath = user.AvatarPath,
                    IsLoginAllowed = user.IsLoginAllowed,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
                return Ok(ApiResponse<UserDto>.SuccessResult(dto, "Profile retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto request)
        {
            try
            {
                var uid = GetCurrentUserId();
                var success = await _userService.UpdateUserAsync(uid, request);
                if (!success) return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                return Ok(ApiResponse<object>.SuccessResult(new { }, "Profile updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid request parameters", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpPost("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var uid = GetCurrentUserId();
                var success = await _userService.ChangePasswordAsync(uid, request);
                if (!success) return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                return Ok(ApiResponse<object>.SuccessResult(new { }, "Password changed successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid request parameters", new List<string> { ex.Message }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpPost("me/avatar")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("No file provided"));

                var uid = GetCurrentUserId();
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var data = ms.ToArray();
                var mime = file.ContentType ?? "application/octet-stream";
                var success = await _userService.SetAvatarAsync(uid, data, mime);
                if (!success) return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                return Ok(ApiResponse<object>.SuccessResult(new { }, "Avatar uploaded successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}/avatar")]
        public async Task<IActionResult> GetAvatar(Guid id)
        {
            try
            {
                var result = await _userService.GetAvatarAsync(id);
                if (result == null) return NotFound();
                return File(result.Value.data, result.Value.mimeType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }
    }

    public class ToggleLoginRequest
    {
        public bool IsAllowed { get; set; }
    }
}
