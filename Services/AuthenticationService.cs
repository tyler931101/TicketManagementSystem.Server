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

        public User? LoginAsync(string email, string password)
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

        public bool RegisterAsync(string username, string password, string email)
        {
            try
            {
                Console.WriteLine($"=== Registration Attempt ===");
                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"Email: {email}");
                
                // Ensure database is created
                Console.WriteLine("Creating database if not exists...");
                _db.Database.EnsureCreated();
                Console.WriteLine("Database created/verified");
                
                // Check existing users
                Console.WriteLine("Checking for existing users...");
                var existingUsers = _db.Users.ToList();
                Console.WriteLine($"Total existing users: {existingUsers.Count}");
                
                foreach (var user in existingUsers)
                {
                    Console.WriteLine($"User: {user.Username}, Email: {user.Email}");
                }

                if (_db.Users.Any(u => u.Username == username))
                {
                    Console.WriteLine($"ERROR: Username '{username}' already exists");
                    return false; // This is the expected false for duplicate
                }

                if (_db.Users.Any(u => u.Email == email))
                {
                    Console.WriteLine($"ERROR: Email '{email}' already exists");
                    return false; // This is the expected false for duplicate
                }

                bool isFirstUser = !_db.Users.Any();
                Console.WriteLine($"Is first user: {isFirstUser}");

                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    Role = isFirstUser ? "Admin" : "User",
                    IsLoginAllowed = true
                };

                Console.WriteLine("Adding new user to database...");
                _db.Users.Add(newUser);
                
                Console.WriteLine("Saving changes...");
                _db.SaveChanges();
                
                Console.WriteLine($"User '{username}' registered successfully");
                Console.WriteLine("=== Registration Complete ===");
                return true;
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"=== Registration Error ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine("=== End Error ===");
                
                // Re-throw the exception so the controller can handle it properly
                // This distinguishes between duplicate errors (return false) and other errors (throw exception)
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }
    }
}
