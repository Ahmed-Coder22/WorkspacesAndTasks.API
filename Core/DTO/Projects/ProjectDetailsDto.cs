using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Projects
{
    public record ProjectDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public UserDto Owner { get; set; }

        public List<ProjectMemberDto> Members { get; set; }

        public bool IsOwner { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
