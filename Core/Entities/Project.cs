using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Project owner
        public string OwnerId { get; set; } = string.Empty;

        public AppUser Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Project members
        public ICollection<ProjectMember> Members { get; set; }
            = new List<ProjectMember>();

        // Tasks belonging to this project
        public ICollection<TaskItem> Tasks { get; set; }
            = new List<TaskItem>();
    }
}
