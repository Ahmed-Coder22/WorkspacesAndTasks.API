using Core.DTO.ProjectMembers;
using Core.DTO.Projects;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ProjectMembersController : ControllerBase
    {
        private readonly IProjectMembers _projectmembers;

        public ProjectMembersController(IProjectMembers projectmembers)
        {
            _projectmembers = projectmembers;
        }


        [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{projectId}/members")]
        public async Task<IActionResult> GetProjectMembers(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _projectmembers.GetProjectMembersAsync(
                projectId,
                userId);

            if(!response.Success)
            {
                return StatusCode(response.StatusCode, response.Message);
            }

            return Ok(response.Data);
        }
        [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("{projectId}/members")]
        public async Task<IActionResult> AddProjectMember(
        int projectId,
        [FromBody] AddProjectMemberDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _projectmembers.AddProjectMemberAsync(
                projectId,
                userId,
                dto);


            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response.Message);
            }

            return CreatedAtAction(
    nameof(GetProjectMembers), 
    new { projectId },
         response.Data); 
             }


        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete("{projectId}/members/{userId}")]
        public async Task<IActionResult> DeleteMember(int projectId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var response = await _projectmembers.DeleteMember(projectId, currentUserId, userId);

            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response.Message);
            }

            return NoContent();
        }
    }

}
