using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Tasks
{
    public record TaskDetailsDto :TaskSummaryDto
    {
        public string? Description { get; set; }
    }
}
