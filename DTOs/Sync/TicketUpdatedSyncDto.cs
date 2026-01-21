namespace TicketManagementSystem.Server.DTOs.Sync
{
    public class TicketUpdatedSyncDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string Priority { get; set; } = "medium";
        public Guid? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
