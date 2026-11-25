using Core.DTO.Projects;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IProjectService
    {
        Task<Pagination<ProjectSummaryDto>> GetUserProjectsAsync(
                string userId,
                PaginationParams paginationParams);


        Task<ServiceResponse<ProjectDetailsDto>> GetProjectDetailsAsync(
            int projectId,
            string userId);



        Task<ServiceResponse<ProjectDetailsDto>> CreateProjectAsync(
       string userId,
       CreateProjectDto dto);

        Task<ServiceResponse<ProjectDetailsDto>> UpdateProjectAsync(
            int projectId,
            string userId,
            UpdateProjectDto dto);

        Task<ServiceResponse<bool>> DeleteProject(
            int projectId,
            string userId);
    }
}
