using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Tasks
{
    public record UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskItemStatus? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public string? AssignedToUserId { get; set; }
    }
}
