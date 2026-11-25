using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Dashboard
{
    public record TaskCountsDto
    {
        public int Total { get; set; }
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
    }
}
