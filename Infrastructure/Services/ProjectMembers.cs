using Core.DTO.ProjectMembers;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Sharing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProjectMembers : IProjectMembers
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ProjectMembers(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<List<ProjectMembersDto>>> GetProjectMembersAsync(
            int projectId,
            string userId)
        {
            // 1. Get project with owner and members
            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            // 2. Check if project exists
            if (project == null)
            {
                return new ServiceResponse<List<ProjectMembersDto>>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // 3. Check authorization
            bool isOwner = project.OwnerId == userId;

            bool isMember = project.Members.Any(m => m.UserId == userId);

            if (!isOwner && !isMember)
            {
                return new ServiceResponse<List<ProjectMembersDto>>
                {
                    Success = false,
                    Message = "You are not authorized to access this project.",
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // 4. Add project owner
            var result = new List<ProjectMembersDto>
            {
        new ProjectMembersDto
              {
            UserId = project.Owner.Id,
            FullName = project.Owner.FullName,
            Email = project.Owner.Email ?? string.Empty,
            Role = "Owner",
            JoinedAt = project.CreatedAt
             }
               };

            // 5. Add regular members
            var members = project.Members.Select(m => new ProjectMembersDto
            {
                UserId = m.User.Id,
                FullName = m.User.FullName,
                Email = m.User.Email ?? string.Empty,
                Role = "Member",
                JoinedAt = m.JoinedAt
            });

            result.AddRange(members);

            // 6. Return success response
            return new ServiceResponse<List<ProjectMembersDto>>
            {
                Success = true,
                Message = "Project members retrieved successfully.",
                Data = result,
                StatusCode = StatusCodes.Status200OK
            };
        }


        public async Task<ServiceResponse<ProjectMembersDto>> AddProjectMemberAsync(
    int projectId,
    string userId,
    AddProjectMemberDto dto)
        {
            // 1. Find the project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return new ServiceResponse<ProjectMembersDto>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // 2. Check if current user is the owner
            if (project.OwnerId != userId)
            {
                return new ServiceResponse<ProjectMembersDto>
                {
                    Success = false,
                    Message = "Only the project owner can add members.",
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // 3. Find the user by email
            var newUser = await _userManager.FindByEmailAsync(dto.Email);

            if (newUser == null)
            {
                return new ServiceResponse<ProjectMembersDto>
                {
                    Success = false,
                    Message = "User with this email is not registered.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // 4. Check if the email belongs to the owner
            if (newUser.Id == project.OwnerId)
            {
                return new ServiceResponse<ProjectMembersDto>
                {
                    Success = false,
                    Message = "Owner is already part of the project.",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // 5. Check if user is already a member
            bool alreadyMember = await _context.ProjectMembers
                .AnyAsync(pm =>
                    pm.ProjectId == projectId &&
                    pm.UserId == newUser.Id);

            if (alreadyMember)
            {
                return new ServiceResponse<ProjectMembersDto>
                {
                    Success = false,
                    Message = "User is already a member.",
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // 6. Create ProjectMember
            var projectMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = newUser.Id,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            await _context.ProjectMembers.AddAsync(projectMember);

            // 7. Save changes
            await _context.SaveChangesAsync();

            // 8. Map entity to DTO
            var memberDto = new ProjectMembersDto
            {
                UserId = newUser.Id,
                FullName = newUser.FullName,
                Email = newUser.Email ?? string.Empty,
                Role = projectMember.Role,
                JoinedAt = projectMember.JoinedAt
            };

            // 9. Return success
            return new ServiceResponse<ProjectMembersDto>
            {
                Success = true,
                Message = "Member added to project successfully.",
                Data = memberDto,
                StatusCode = StatusCodes.Status201Created
            };
        }


        public async Task<ServiceResponse<bool>> DeleteMember(int projectId, string currentuserId, string memberUserId)
        {
            // 1. Find the project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Project not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // 2. Check if current user is the owner
            if (project.OwnerId != currentuserId)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Only the project owner can delete members.",
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // 3. Find the member to remove
            var memberToDelete = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == projectId &&
                    pm.UserId == memberUserId);

            if (memberToDelete == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Member not found in this project.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // 4. Unassign all tasks assigned to this member
            var assignedTasks = await _context.Tasks
                .Where(t =>
                    t.ProjectId == projectId &&
                    t.AssignedToUserId == memberUserId)
                .ToListAsync();

            foreach (var task in assignedTasks)
            {
                task.AssignedToUserId = null;
            }

            // 5. Remove member from project
            _context.ProjectMembers.Remove(memberToDelete);

            // 6. Save changes
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Member removed successfully.",
                Data = true,
                StatusCode = StatusCodes.Status204NoContent
            };

        }
    }
}
