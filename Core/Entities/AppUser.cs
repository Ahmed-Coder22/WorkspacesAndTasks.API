using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Projects owned by this user
        public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();

        // Projects this user belongs to
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

        // Tasks assigned to this user
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
