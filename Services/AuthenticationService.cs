using System.Linq;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;

namespace TicketManagementSystem.Server.Services
{
    public class AuthenticationService
    {
        private readonly AppDbContext _db;

        public AuthenticationService(AppDbContext db)
        {
            _db = db;
        }

        public User? Login(string email, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return null;

            if (user.Password != password)
                return null;

            if (!user.IsLoginAllowed)
                return null;

            return user;
        }

        public bool Register(string username, string password, string email)
        {
            try
            {
                if (_db.Users.Any(u => u.Username == username))
                    return false;

                if (_db.Users.Any(u => u.Email == email))
                    return false;

                bool isFirstUser = !_db.Users.Any();

                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    Role = isFirstUser ? "Admin" : "User",
                    IsLoginAllowed = true
                };

                _db.Users.Add(newUser);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return false to indicate failure
                return false;
            }
        }
    }
}
