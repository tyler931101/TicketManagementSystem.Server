using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Ticket> GetAll()
        {
            return _db.Tickets
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public IEnumerable<Ticket> GetByUser(string username)
        {
            return _db.Tickets
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUser != null && t.AssignedUser.Username == username)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public void Add(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
            _db.SaveChanges();
        }

        public void Update(Ticket ticket)
        {
            _db.Tickets.Update(ticket);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var existing = _db.Tickets.Find(id);
            if (existing != null)
            {
                _db.Tickets.Remove(existing);
                _db.SaveChanges();
            }
        }
    }
}
