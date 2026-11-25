using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ProjectMember
    {
        public int ProjectId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";
        public DateTime JoinedAt { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;

        public AppUser User { get; set; } = null!;
    }
}
