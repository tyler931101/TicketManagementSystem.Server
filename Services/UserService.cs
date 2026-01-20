/* 
PSEUDOCODE / PLAN:
1. Identify cause: 'AsNoTracking' is an EF Core extension method defined in the Microsoft.EntityFrameworkCore namespace.
2. Fix: add the missing using directive 'using Microsoft.EntityFrameworkCore;' at the top of the file so the extension method is available.
3. Keep existing logic unchanged; no signature or behavior modifications required.
4. Provide the updated file content with the new using added and include this pseudocode as a comment for documentation.
*/

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<User>> GetTicketUsersAsync()
        {
            return await _db.Users
                     .OrderBy(u => u.Username)
                     .ToListAsync();
        }

        public async Task<PagedResponseDto<User>> GetAllUsersAsync(PagedRequestDto request)
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
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.PhoneNumber.ToLower().Contains(searchTerm) ||
                    u.Address.ToLower().Contains(searchTerm));
            }

            // Optimized count query - only execute when needed
            var totalRecords = await query.CountAsync();

            // Apply pagination with optimized ordering
            var users = await query
                .OrderBy(u => u.Username)  // Consider adding index on Username
                .ThenBy(u => u.Id)       // Secondary sort for consistent results
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new User  // Projection reduces data transfer
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Role = u.Role,
                    IsLoginAllowed = u.IsLoginAllowed
                }).AsNoTracking()  // Read-only query optimization
                .ToListAsync();

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

        public async Task SetLoginAllowedAsync(User user, bool isAllowed)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.IsLoginAllowed = isAllowed;
                await _db.SaveChangesAsync();
            }
        }
    }
}
