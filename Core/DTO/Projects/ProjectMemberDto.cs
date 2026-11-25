using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Projects
{
    public record ProjectMemberDto
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
