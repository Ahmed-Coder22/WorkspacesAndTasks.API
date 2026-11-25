using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Tasks
{
    public record CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

        public DateTime? DueDate { get; set; }

        public string? AssignedToUserId { get; set; }
    }
}
