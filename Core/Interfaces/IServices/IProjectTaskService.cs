using Core.DTO.Tasks;
using Core.Enums;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.IServices
{
    public interface IProjectTaskService
    {
        Task<ServiceResponse<Pagination<TaskSummaryDto>>> GetProjectTasksAsync(
       int projectId,
       string userId,
       PaginationParams pagination,
       TaskItemStatus? status,
       string? assignedToUserId
           );

        Task<ServiceResponse<TaskDetailsDto>> CreateTaskAsync(
      int projectId,
      string userId,
      CreateTaskDto dto
          );

        Task<ServiceResponse<TaskDetailsDto>> GetTaskByIdAsync(
       int taskId,
       string userId
          );


        Task<ServiceResponse<TaskDetailsDto>> UpdateTaskAsync(
      int taskId,
      string userId,
      UpdateTaskDto dto);

        Task<ServiceResponse<bool>> DeleteTask(int taskId, string userId);

        Task<ServiceResponse<Pagination<MyTaskDto>>> GetMyTasksAsync(
        string userId,
        PaginationParams pagination,
        TaskItemStatus? status);
    }
}
