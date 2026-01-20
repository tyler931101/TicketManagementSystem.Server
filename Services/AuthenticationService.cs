using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;

namespace TicketManagementSystem.Server.Services
{
    public class AuthenticationService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthenticationService(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<(User? User, string? Token)> LoginAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return (null, null);

            // Verify password hash
            // Fallback for existing plain text passwords (for migration purposes if needed, but better to enforce hash)
            // Ideally we should re-hash if it was plain text, but for now assuming all new users are hashed.
            // If the password length is < 60, it's likely plain text (BCrypt hash is 60 chars).
            bool isValid = false;
            if (user.Password.Length < 60)
            {
                // Legacy plain text check
                isValid = user.Password == password;
                if (isValid)
                {
                    // Upgrade to hash
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }

            if (!isValid)
                return (null, null);

            if (!user.IsLoginAllowed)
                return (user, null);

            var token = GenerateJwtToken(user);
            return (user, token);
        }

        public async Task<bool> RegisterAsync(string username, string password, string email)
        {
            try
            {
                // Ensure database is created
                await _db.Database.EnsureCreatedAsync();

                if (await _db.Users.AnyAsync(u => u.Username == username || u.Email == email))
                {
                    return false;
                }

                bool isFirstUser = !await _db.Users.AnyAsync();

                var newUser = new User
                {
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Email = email,
                    Role = isFirstUser ? "Admin" : "User",
                    IsLoginAllowed = true
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Registration error: {ex.Message}");
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:ExpireDays"] ?? "7")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
