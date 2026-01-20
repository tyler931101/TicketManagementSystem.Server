using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;
using TicketManagementSystem.Server.DTOs.Auth;
using TicketManagementSystem.Server.DTOs.Common;
using TicketManagementSystem.Server.DTOs.Users;
using TicketManagementSystem.Server.DTOs.Tickets;

namespace TicketManagementSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tickets = await _ticketService.GetAllAsync();
                var ticketDtos = tickets.Select(t => new TicketDto
                { 
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    Priority = "Medium", // Default priority since model doesn't have it
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    AssignedUserId = t.AssignedUserId?.ToString(),
                    AssignedUser = t.AssignedUser == null ? null : new DTOs.Users.UserDto
                    { 
                        Id = t.AssignedUser.Id,
                        Username = t.AssignedUser.Username,
                        Email = t.AssignedUser.Email,
                        Role = t.AssignedUser.Role,
                        AvatarPath = t.AssignedUser.AvatarPath,
                        IsLoginAllowed = t.AssignedUser.IsLoginAllowed,
                        CreatedAt = t.AssignedUser.CreatedAt,
                        UpdatedAt = t.AssignedUser.UpdatedAt
                    }
                }).ToList();
                
                return Ok(ApiResponse<List<TicketDto>>.SuccessResult(ticketDtos, "Tickets retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TicketDto>>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetByUser(string username)
        {
            try
            {
                var tickets = await _ticketService.GetByUserAsync(username);
                return Ok(tickets.Select(t => new 
                { 
                    t.Id, 
                    t.Title, 
                    t.Description, 
                    t.Status, 
                    t.DueDate, 
                    t.CreatedAt, 
                    t.UpdatedAt,
                    AssignedUserId = t.AssignedUserId,
                    AssignedUser = t.AssignedUser == null ? null : new 
                    { 
                        t.AssignedUser.Id, 
                        t.AssignedUser.Username 
                    }
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
        {
            try
            {
                var ticket = new Ticket
                {
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    DueDate = request.DueDate,
                    AssignedUserId = request.AssignedUserId
                };

                await _ticketService.AddAsync(ticket);
                return Ok(ApiResponse<object>.SuccessResult(new { }, "Ticket created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Server error", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTicketRequest request)
        {
            try
            {
                var ticket = new Ticket
                {
                    Id = Guid.Parse(id),
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    DueDate = request.DueDate,
                    AssignedUserId = request.AssignedUserId,
                    UpdatedAt = DateTime.Now
                };

                var success = await _ticketService.UpdateAsync(ticket);
                if (!success)
                {
                    return NotFound(new { message = "Ticket not found" });
                }
                return Ok(new { message = "Ticket updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _ticketService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Ticket not found" });
                }
                return Ok(new { message = "Ticket deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }

    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "To Do";
        public DateTime DueDate { get; set; }
        public Guid? AssignedUserId { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public Guid? AssignedUserId { get; set; }
    }
}
