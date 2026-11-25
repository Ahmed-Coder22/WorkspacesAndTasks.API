using Core.DTO.Tasks;
using Core.Enums;
using Core.Interfaces.IServices;
using Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectTasksController : ControllerBase
    {
        private readonly IProjectTaskService _taskService;

        public ProjectTasksController(IProjectTaskService taskService)
        {
            _taskService = taskService;
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IEnumerable<TaskSummaryDto>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetProjectTasks(
            int projectId,
            [FromQuery] PaginationParams pagination,
            [FromQuery] TaskItemStatus? status,
            [FromQuery] string? assignedToUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.GetProjectTasksAsync(
                projectId,
                userId,
                pagination,
                status,
                assignedToUserId
            );
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response.Message);
            }
            return Ok(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(
        int projectId,
        [FromBody] CreateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.CreateTaskAsync(
                projectId,
                userId,
                dto
            );

            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { taskId = response.Data!.Id },
                response
            );
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.GetTaskByIdAsync(
                taskId,
                userId
            );
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response.Data);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(
        int taskId,
        [FromBody] UpdateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.UpdateTaskAsync(
                taskId,
                userId,
                dto
            );
            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);
            return Ok(response.Data);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.DeleteTask(taskId, userId);

            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return NoContent();
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTasks(
            [FromQuery] PaginationParams pagination,
            [FromQuery] TaskItemStatus? status )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _taskService.GetMyTasksAsync(
                userId,
                pagination,
                status
            );

            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            return Ok(response.Data);

        }
    }
}
