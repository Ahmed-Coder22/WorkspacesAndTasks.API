using Core.DTO.Dashboard;
using Core.DTO.Projects;
using Core.Enums;
using Core.Interfaces.IServices;
using Core.Sharing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<DashboardDto>> GetDashboardAsync(string userId)
        {
            // 1. Get user's projects (owned or member)
            var projects = await _context.Projects
                .AsNoTracking()
                .Where(p =>
                    p.OwnerId == userId ||
                    p.Members.Any(m => m.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner.FullName,
                    IsOwner = p.OwnerId == userId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            // 2. Get tasks assigned to the current user
            var taskCounts = await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == userId)
                .GroupBy(t => 1)
                .Select(g => new TaskCountsDto
                {
                    Total = g.Count(),

                    ToDo = g.Count(t =>
                        t.Status == TaskItemStatus.ToDo),

                    InProgress = g.Count(t =>
                        t.Status == TaskItemStatus.InProgress),

                    Completed = g.Count(t =>
                        t.Status == TaskItemStatus.Completed)
                })
                .FirstOrDefaultAsync();

            // 3. Build dashboard response
            var dashboard = new DashboardDto
            {
                Projects = projects,
                TaskCounts = taskCounts ?? new TaskCountsDto()
            };

            return new ServiceResponse<DashboardDto>
            {
                Success = true,
                Message = "Dashboard retrieved successfully.",
                Data = dashboard,
                StatusCode = 200
            };
        }
    }
}
