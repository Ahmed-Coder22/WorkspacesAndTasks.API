using Core.DTO.Projects;
using Core.Interfaces.Repositories;
using Core.Sharing;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(Pagination<ProjectSummaryDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserProjects(
            [FromQuery] PaginationParams paginationParams)
        {
            // Get current authenticated user's ID.

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _projectService.GetUserProjectsAsync(
                userId,
                paginationParams
            );

            return Ok(result);
        }

        [ProducesResponseType(typeof(ProjectDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{projectId:int}")]

        public async Task<IActionResult> GetProjectById(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _projectService.GetProjectDetailsAsync(
                projectId, userId);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [ProducesResponseType(typeof(ProjectDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _projectService.CreateProjectAsync(userId, dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result.Message);

            return CreatedAtAction(
                nameof(GetProjectById),
                new { projectId = result.Data!.Id },
                result.Data);
        }

        [ProducesResponseType(typeof(ProjectDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(
         int projectId,
         UpdateProjectDto dto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _projectService.UpdateProjectAsync(
                projectId,
                userId,
                dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Data);

        }

        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(
            int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _projectService.DeleteProject(
                projectId,
                userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result.Message);

            return NoContent();
        }
    }
}
