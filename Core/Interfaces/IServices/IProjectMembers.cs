using Core.DTO.ProjectMembers;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IProjectMembers
    {
        Task<ServiceResponse<List<ProjectMembersDto>>> GetProjectMembersAsync(
           int projectId,
           string userId);


        Task<ServiceResponse<ProjectMembersDto>> AddProjectMemberAsync(
           int projectId,
           string userId,
           AddProjectMemberDto dto);

        Task<ServiceResponse<bool>> DeleteMember(int projectId, string currentuserId,string memberUserId);
    }
}
