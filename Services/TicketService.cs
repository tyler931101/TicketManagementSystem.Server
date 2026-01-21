using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.Services.Sync;
using TicketManagementSystem.Server.DTOs.Sync;

namespace TicketManagementSystem.Server.Services
{
    public class TicketService
    {
        private readonly AppDbContext _db;
        private readonly ISyncPublisher _syncPublisher;

        public TicketService(AppDbContext db, ISyncPublisher syncPublisher)
        {
            _db = db;
            _syncPublisher = syncPublisher;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _db.Tickets
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByUserAsync(string username)
        {
            return await _db.Tickets
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUser != null && t.AssignedUser.Username == username)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        private static string NormalizeStatus(string status)
        {
            var s = (status ?? "").Trim().ToLowerInvariant();
            return s switch
            {
                "to do" => "todo",
                "todo" => "todo",
                "to_do" => "todo",
                "in progress" => "in_progress",
                "in_progress" => "in_progress",
                "resolved" => "resolved",
                "testing" => "testing",
                "closed" => "closed",
                "done" => "done",
                _ => "todo"
            };
        }

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            var dto = new TicketCreatedSyncDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = NormalizeStatus(ticket.Status),
                Priority = "medium",
                DueDate = ticket.DueDate,
                AssignedTo = ticket.AssignedUserId
            };
            await _syncPublisher.PublishTicketCreatedAsync(dto);
            return ticket;
        }

        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            var existing = await _db.Tickets.FindAsync(ticket.Id);
            if (existing == null)
                return false;

            ticket.CreatedAt = existing.CreatedAt;
            ticket.UpdatedAt = DateTime.Now;

            _db.Entry(existing).CurrentValues.SetValues(ticket);
            await _db.SaveChangesAsync();
            var dto = new TicketUpdatedSyncDto
            {
                Id = ticket.Id.ToString(),
                Title = ticket.Title,
                Description = ticket.Description,
                Status = NormalizeStatus(ticket.Status),
                Priority = "medium",
                DueDate = ticket.DueDate,
                AssignedTo = ticket.AssignedUserId
            };
            await _syncPublisher.PublishTicketUpdatedAsync(dto);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out var ticketId))
                return false;

            var existing = await _db.Tickets.FindAsync(ticketId);
            if (existing != null)
            {
                _db.Tickets.Remove(existing);
                await _db.SaveChangesAsync();
                var dto = new TicketDeletedSyncDto { Id = id };
                await _syncPublisher.PublishTicketDeletedAsync(dto);
                return true;
            }
            return false;
        }

        public async Task<bool> MoveAsync(string id, string status)
        {
            if (!Guid.TryParse(id, out var ticketId))
                return false;
            var existing = await _db.Tickets.FindAsync(ticketId);
            if (existing == null)
                return false;
            existing.Status = status;
            existing.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            var dto = new TicketStatusChangedSyncDto { Id = id, Status = NormalizeStatus(status) };
            await _syncPublisher.PublishTicketStatusChangedAsync(dto);
            return true;
        }
    }
}
