using Core.DTO.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Dashboard
{
    public record DashboardDto
    {
        public List<ProjectSummaryDto> Projects { get; set; } = new();

        public TaskCountsDto TaskCounts { get; set; } = new();
    }
}
