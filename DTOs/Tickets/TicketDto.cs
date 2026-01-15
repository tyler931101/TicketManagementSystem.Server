using System.ComponentModel.DataAnnotations;
using TicketManagementSystem.Server.DTOs.Users;

namespace TicketManagementSystem.Server.DTOs.Tickets
{
    public class TicketDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }

        public UserDto? AssignedUser { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTicketDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Due date must be in the future")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = "Medium";

        public string? AssignedUserId { get; set; }
    }

    public class UpdateTicketDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }
    }

    public class TicketFilterDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? AssignedUserId { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    // Custom validation attribute for future dates
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime <= DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage ?? "Date must be in the future");
                }
            }
            return ValidationResult.Success;
        }
    }
}
