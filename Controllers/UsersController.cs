using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;

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
        public IActionResult GetAll()
        {
            try
            {
                var users = _userService.GetAll();
                return Ok(users.Select(u => new 
                { 
                    u.Id, 
                    u.Username, 
                    u.Email, 
                    u.Role, 
                    u.IsLoginAllowed 
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
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
