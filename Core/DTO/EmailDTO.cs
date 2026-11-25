using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public record EmailDTO(string To, string From, string Subject, string Content);
}
