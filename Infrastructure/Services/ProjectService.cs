using AutoMapper;
using Core.DTO.Projects;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Sharing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<ProjectDetailsDto>> GetProjectDetailsAsync(int projectId, string userId)
        {
            var project = await _context.Projects
        .AsNoTracking()
        .Include(p => p.Owner)
        .Include(p => p.Members)
            .ThenInclude(m => m.User)
        .FirstOrDefaultAsync(p =>
            p.Id == projectId &&
            (p.OwnerId == userId ||
             p.Members.Any(m => m.UserId == userId)));

            if (project == null)
            {
                return new ServiceResponse<ProjectDetailsDto>
                {
                    Success = false,
                    Message = "Project not found or you don't have access to it.",
                    StatusCode = 404
                };
            }

            var result = _mapper.Map<ProjectDetailsDto>(project);

            // Determine whether the current user is the owner.
            result.IsOwner = project.OwnerId == userId;

            // Add owner to members list (not stored in database).
            result.Members.Insert(0, new ProjectMemberDto
            {
                UserId = project.Owner.Id,
                FullName = project.Owner.FullName,
                Role = "Owner",
                JoinedAt = project.CreatedAt
            });

            return new ServiceResponse<ProjectDetailsDto>
            {
                Success = true,
                Data = result,
                Message = "Project retrieved successfully.",
                StatusCode = 200
            };
        }

        public async Task<Pagination<ProjectSummaryDto>> GetUserProjectsAsync(string userId, PaginationParams paginationParams)
        {
            // 1. Get projects owned by the user
            // OR projects where the user is a member.

            var query = _context.Projects
                .AsNoTracking()
                .Where(p =>
                    p.OwnerId == userId ||
                    p.Members.Any(m => m.UserId == userId));

            // 2. Get total count before pagination.

            var totalCount = await query.CountAsync();

            // 3. Apply pagination and projection.

            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
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

            // 4. Return paginated result.

            return new Pagination<ProjectSummaryDto>(
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount,
                projects
            );
        }


        public async Task<ServiceResponse<ProjectDetailsDto>> CreateProjectAsync(string userId, CreateProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var createdProject = await _context.Projects
                       .AsNoTracking()
                       .Include(p => p.Owner)
                       .Include(p => p.Members)
                           .ThenInclude(m => m.User)
                       .FirstOrDefaultAsync(p => p.Id == project.Id);

            var result = _mapper.Map<ProjectDetailsDto>(createdProject);
            result.IsOwner = true;

            return new ServiceResponse<ProjectDetailsDto>
            {
                Success = true,
                Data = result,
                Message = "Project created successfully",
                StatusCode = 201
            };
        }

        public async Task<ServiceResponse<bool>> DeleteProject(int projectId, string userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Project not found",
                    StatusCode = 404
                };

            if (project.OwnerId != userId)
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Only the project owner can delete this project.",
                    StatusCode = 403
                };

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Project deleted successfully",
                StatusCode = 204,
            };

        }

        public async Task<ServiceResponse<ProjectDetailsDto>> UpdateProjectAsync(int projectId, string userId, UpdateProjectDto dto)
        {
            // 1. Get project with its navigation properties

            var project = await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            // 2. Check if project exists

            if (project == null)
                return new ServiceResponse<ProjectDetailsDto>
                {
                    Success = false,
                    Message = "Project not found",
                    StatusCode = 404
                };
            // 3. Check ownership

            if (project.OwnerId != userId)
                return new ServiceResponse<ProjectDetailsDto>
                {
                    Success = false,
                    Message = "Only the project owner can update this project.",
                    StatusCode = 403
                };

            // 4. Update properties

            project.Name = dto.Name;
            project.Description = dto.Description;

            // 5. Save changes

            await _context.SaveChangesAsync();

            // 6. Map to DTO

            var result = _mapper.Map<ProjectDetailsDto>(project);

            result.IsOwner = true;

            return new ServiceResponse<ProjectDetailsDto>
            {
                Success = true,
                Data = result,
                Message = "Project updated successfully",
            };
        }
    }
    }
