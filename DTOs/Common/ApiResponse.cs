using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TicketManagementSystem.Server.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class PagedResponse<T> : ApiResponse<List<T>>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public static PagedResponse<T> Create(List<T> data, int page, int pageSize, int totalCount, string message = "Data retrieved successfully")
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return new PagedResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }
    }

    public class ValidationErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "Validation failed";
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
