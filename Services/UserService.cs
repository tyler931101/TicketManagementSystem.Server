using System.Collections.Generic;
using System.Linq;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;

namespace TicketManagementSystem.Server.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<User> GetAll()
        {
            return _db.Users
                     .OrderBy(u => u.Username)
                     .ToList();
        }

        public void SetLoginAllowed(User user, bool isAllowed)
        {
            var existingUser = _db.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.IsLoginAllowed = isAllowed;
                _db.SaveChanges();
            }
        }
    }
}
