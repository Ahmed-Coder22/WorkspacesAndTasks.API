using Core.DTO.Dashboard;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.IServices
{
    public interface IDashboardService
    {
        Task<ServiceResponse<DashboardDto>> GetDashboardAsync(string userId);
    }
}
