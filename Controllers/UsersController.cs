using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;
using TicketManagementSystem.Server.DTOs.Users;
using TicketManagementSystem.Server.DTOs.Common;

namespace TicketManagementSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ResponseCache(Duration = 30)] // Cache for 30 seconds
        public IActionResult GetAllUsers([FromQuery] PagedRequestDto request)
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
                var result = _userService.GetAllUsersAsync(request);
                stopwatch.Stop();

                // Add performance metadata
                Response.Headers.Add("X-Response-Time", stopwatch.ElapsedMilliseconds.ToString());
                Response.Headers.Add("X-Total-Records", result.TotalRecords.ToString());

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
        public IActionResult GetTicketUsers()
        {
            try
            {
                var users = _userService.GetTicketUsersAsync();
                
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
        public IActionResult ToggleLogin(Guid id, [FromBody] ToggleLoginRequest request)
        {
            try
            {
                var user = new User { Id = id };
                _userService.SetLoginAllowed(user, request.IsAllowed);
                return Ok(new { message = "Login permission updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }

    public class ToggleLoginRequest
    {
        public bool IsAllowed { get; set; }
    }
}
