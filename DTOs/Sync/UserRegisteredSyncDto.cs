using System;

namespace TicketManagementSystem.Server.DTOs.Sync
{
    public class UserRegisteredSyncDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
