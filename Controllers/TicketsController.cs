using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services;

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
        public IActionResult GetAll()
        {
            try
            {
                var tickets = _ticketService.GetAll();
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

        [HttpGet("user/{username}")]
        public IActionResult GetByUser(string username)
        {
            try
            {
                var tickets = _ticketService.GetByUser(username);
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
        public IActionResult Create([FromBody] CreateTicketRequest request)
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

                _ticketService.Add(ticket);
                return Ok(new { message = "Ticket created successfully", ticketId = ticket.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateTicketRequest request)
        {
            try
            {
                var ticket = new Ticket
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    DueDate = request.DueDate,
                    AssignedUserId = request.AssignedUserId,
                    UpdatedAt = DateTime.Now
                };

                _ticketService.Update(ticket);
                return Ok(new { message = "Ticket updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _ticketService.Delete(id);
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
        public int? AssignedUserId { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int? AssignedUserId { get; set; }
    }
}
