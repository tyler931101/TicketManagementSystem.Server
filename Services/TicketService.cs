using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;

namespace TicketManagementSystem.Server.Services
{
    public class TicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db)
        {
            _db = db;
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

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> UpdateAsync(Ticket ticket)
        {
            var existing = await _db.Tickets.FindAsync(ticket.Id);
            if (existing == null)
                return false;

            _db.Entry(existing).CurrentValues.SetValues(ticket);
            await _db.SaveChangesAsync();
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
                return true;
            }
            return false;
        }
    }
}
