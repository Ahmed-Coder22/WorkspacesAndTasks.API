using Core.DTO.Projects;
using Core.DTO.Tasks;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IServices;
using Core.Sharing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly AppDbContext _context;

        public ProjectTaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<Pagination<TaskSummaryDto>>> GetProjectTasksAsync(
            int projectId,
            string userId,
            PaginationParams pagination,
            TaskItemStatus? status,
            string? assignedToUserId)
        {
            // 1. Check whether the user can access the project

            var hasAccess = await _context.Projects.AnyAsync(p =>
                p.Id == projectId &&
                (p.OwnerId == userId ||
                 p.Members.Any(m => m.UserId == userId)));

            if (!hasAccess)
            {
                return new ServiceResponse<Pagination<TaskSummaryDto>>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = 404
                };
            }

            // 2. Get project tasks

            var query = _context.Tasks
                .Where(t => t.ProjectId == projectId);

            // 3. Apply status filter

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // 4. Apply assigned user filter

            if (!string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(t =>
                    t.AssignedToUserId == assignedToUserId);
            }

            // 5. Get total count before pagination

            var totalCount = await query.CountAsync();

            // 6. Sort newest first

            query = query.OrderByDescending(t => t.CreatedAt);

            // 7. Apply pagination and projection

            var tasks = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(t => new TaskSummaryDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,

                    AssignedTo = t.AssignedToUser == null
                        ? null
                        : new UserDto
                        {
                            Id = t.AssignedToUser.Id,
                            FullName = t.AssignedToUser.FullName
                        }
                })
                .ToListAsync();

            // 8. Create paginated response

            var paginatedTasks = new Pagination<TaskSummaryDto>(
                pagination.PageNumber,
                pagination.PageSize,
                totalCount,
                tasks
            );

            return new ServiceResponse<Pagination<TaskSummaryDto>>
            {
                Success = true,
                Message = "Tasks retrieved successfully.",
                StatusCode = 200,
                Data = paginatedTasks
            };
        }

        public async Task<ServiceResponse<TaskDetailsDto>> CreateTaskAsync(
       int projectId,
       string userId,
       CreateTaskDto dto)
        {
            // 1. Check if project exists and user has access

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = 404
                };
            }

            var hasAccess = project.OwnerId == userId ||
                await _context.ProjectMembers.AnyAsync(m =>
                    m.ProjectId == projectId &&
                    m.UserId == userId);

            if (!hasAccess)
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = 404
                };
            }

            // 2. Validate Priority

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority))
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Invalid task priority.",
                    StatusCode = 400
                };
            }

            // 3. Validate Status

            if (!Enum.IsDefined(typeof(TaskItemStatus), dto.Status))
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Invalid task status.",
                    StatusCode = 400
                };
            }

            // 4. Validate Assigned User

            AppUser? assignedUser = null;

            if (!string.IsNullOrWhiteSpace(dto.AssignedToUserId))
            {
                assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.AssignedToUserId);

                if (assignedUser == null)
                {
                    return new ServiceResponse<TaskDetailsDto>
                    {
                        Success = false,
                        Message = "Assigned user not found.",
                        StatusCode = 400
                    };
                }

                var isParticipant = project.OwnerId == dto.AssignedToUserId ||
                    await _context.ProjectMembers.AnyAsync(m =>
                        m.ProjectId == projectId &&
                        m.UserId == dto.AssignedToUserId);

                if (!isParticipant)
                {
                    return new ServiceResponse<TaskDetailsDto>
                    {
                        Success = false,
                        Message = "Assigned user is not a participant of this project.",
                        StatusCode = 400
                    };
                }
            }

            // 5. Create Task Entity

            var task = new TaskItem
            {
                ProjectId = projectId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Priority = dto.Priority,
                Status = dto.Status,
                DueDate = dto.DueDate,
                AssignedToUserId = assignedUser?.Id,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Save Task

            await _context.Tasks.AddAsync(task);

            await _context.SaveChangesAsync();

            // 7. Map Entity to DTO

            var taskDetails = new TaskDetailsDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Status = task.Status,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,

                AssignedTo = assignedUser == null
                    ? null
                    : new UserDto
                    {
                        Id = assignedUser.Id,
                        FullName = assignedUser.FullName
                    }
            };

            // 8. Return Response

            return new ServiceResponse<TaskDetailsDto>
            {
                Success = true,
                Message = "Task created successfully.",
                StatusCode = 201,
                Data = taskDetails
            };
        }

        public async Task<ServiceResponse<TaskDetailsDto>> GetTaskByIdAsync(
        int taskId,
        string userId)
        {
            // 1. Retrieve the task and verify user access

            var task = await _context.Tasks
                .Where(t =>
                    t.Id == taskId &&
                    (t.Project.OwnerId == userId ||
                     t.Project.Members.Any(m => m.UserId == userId)))
                .Select(t => new TaskDetailsDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,

                    AssignedTo = t.AssignedToUser == null
                        ? null
                        : new UserDto
                        {
                            Id = t.AssignedToUser.Id,
                            FullName = t.AssignedToUser.FullName
                        }
                })
                .FirstOrDefaultAsync();

            // 2. Check whether task exists and user has permission

            if (task == null)
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Task not found.",
                    StatusCode = 404
                };
            }

            // 3. Return task details

            return new ServiceResponse<TaskDetailsDto>
            {
                Success = true,
                Message = "Task retrieved successfully.",
                StatusCode = 200,
                Data = task
            };
        }

        public async Task<ServiceResponse<TaskDetailsDto>> UpdateTaskAsync(
        int taskId,
        string userId,
        UpdateTaskDto dto)
        {
            // 1. Retrieve the task

            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Task not found.",
                    StatusCode = 404
                };
            }

            // 2. Check project access

            var hasAccess = task.Project.OwnerId == userId ||
                await _context.ProjectMembers.AnyAsync(m =>
                    m.ProjectId == task.ProjectId &&
                    m.UserId == userId);

            if (!hasAccess)
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Task not found.",
                    StatusCode = 404
                };
            }

            // 3. Validate Status

            if (!Enum.IsDefined(typeof(TaskItemStatus), dto.Status!.Value))
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Invalid task status.",
                    StatusCode = 400
                };
            }

            // 4. Validate Priority

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority!.Value))
            {
                return new ServiceResponse<TaskDetailsDto>
                {
                    Success = false,
                    Message = "Invalid task priority.",
                    StatusCode = 400
                };
            }

            // 5. Validate Assigned User

            AppUser? assignedUser = null;

            if (!string.IsNullOrWhiteSpace(dto.AssignedToUserId))
            {
                assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.AssignedToUserId);

                if (assignedUser == null)
                {
                    return new ServiceResponse<TaskDetailsDto>
                    {
                        Success = false,
                        Message = "Assigned user not found.",
                        StatusCode = 400
                    };
                }

                var isParticipant = task.Project.OwnerId == dto.AssignedToUserId ||
                    await _context.ProjectMembers.AnyAsync(m =>
                        m.ProjectId == task.ProjectId &&
                        m.UserId == dto.AssignedToUserId);

                if (!isParticipant)
                {
                    return new ServiceResponse<TaskDetailsDto>
                    {
                        Success = false,
                        Message = "Assigned user is not a participant of this project.",
                        StatusCode = 400
                    };
                }
            }

            // 6. Update editable properties

            task.Title = dto.Title.Trim();
            task.Description = dto.Description;
            task.Status = dto.Status.Value;
            task.Priority = dto.Priority.Value;
            task.DueDate = dto.DueDate;
            task.AssignedToUserId = assignedUser?.Id;

            // 7. Save changes

            await _context.SaveChangesAsync();

            // 8. Map updated task to DTO

            var taskDetails = new TaskDetailsDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,

                AssignedTo = assignedUser == null
                    ? null
                    : new UserDto
                    {
                        Id = assignedUser.Id,
                        FullName = assignedUser.FullName
                    }
            };

            // 9. Return response

            return new ServiceResponse<TaskDetailsDto>
            {
                Success = true,
                Message = "Task updated successfully.",
                StatusCode = 200,
                Data = taskDetails
            };
        }

        public async Task<ServiceResponse<bool>> DeleteTask(int taskId, string userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Task not found.",
                    StatusCode = 404
                };
            }

            // Check if the user is the project owner
            if (task.Project.OwnerId != userId)
            {
                // Check if the user is a project member
                var isMember = await _context.ProjectMembers.AnyAsync(m =>
                    m.ProjectId == task.ProjectId &&
                    m.UserId == userId);

                if (isMember)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Only the project owner can delete tasks.",
                        StatusCode = 403
                    };
                }

                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Task not found.",
                    StatusCode = 404
                };
            }

            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Task deleted successfully.",
                StatusCode = 204
            };
        }

        public async Task<ServiceResponse<Pagination<MyTaskDto>>> GetMyTasksAsync(string userId, PaginationParams pagination, TaskItemStatus? status)
        {
            // Validate status
            if (status.HasValue &&
                !Enum.IsDefined(typeof(TaskItemStatus), status.Value))
            {
                return new ServiceResponse<Pagination<MyTaskDto>>
                {
                    Success = false,
                    Message = "Invalid task status.",
                    StatusCode = 400
                };
            }

            // Get tasks assigned to the current user
            var query = _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == userId);

            // Apply optional status filter
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply ordering, pagination and projection
            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(t => new MyTaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.Name,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var result = new Pagination<MyTaskDto>(
       pagination.PageNumber,
        pagination.PageSize,
                totalCount,
           tasks
                     );

            return new ServiceResponse<Pagination<MyTaskDto>>
            {
                Success = true,
                Message = "My tasks retrieved successfully.",
                Data = result,
                StatusCode = 200
            };
        }
    }
}
