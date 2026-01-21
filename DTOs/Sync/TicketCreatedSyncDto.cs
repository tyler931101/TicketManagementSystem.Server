using System;

namespace TicketManagementSystem.Server.DTOs.Sync
{
    public class TicketCreatedSyncDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "todo";
        public string Priority { get; set; } = "medium";
        public Guid? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
