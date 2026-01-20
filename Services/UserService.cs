using System.Collections.Generic;
using System.Linq;
using TicketManagementSystem.Server.Data;
using TicketManagementSystem.Server.Models;
using TicketManagementSystem.Server.DTOs.Common;

namespace TicketManagementSystem.Server.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<User> GetTicketUsersAsync()
        {
            return _db.Users
                     .OrderBy(u => u.Username)
                     .ToList();
        }

        public PagedResponseDto<User> GetAllUsersAsync(PagedRequestDto request)
        {
            // Validate request parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;
            
            var query = _db.Users.AsQueryable();

            // Apply search filter with optimized conditions
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm));
            }

            // Optimized count query - only execute when needed
            var totalRecords = query.Count();

            // Apply pagination with optimized ordering
            var users = query
                .OrderBy(u => u.Username)  // Consider adding index on Username
                .ThenBy(u => u.Id)       // Secondary sort for consistent results
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new User  // Projection reduces data transfer
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsLoginAllowed = u.IsLoginAllowed
                })
                .AsNoTracking()  // Read-only query optimization
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PagedResponseDto<User>
            {
                Data = users,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                HasNextPage = request.PageNumber < totalPages,
                HasPreviousPage = request.PageNumber > 1
            };
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
