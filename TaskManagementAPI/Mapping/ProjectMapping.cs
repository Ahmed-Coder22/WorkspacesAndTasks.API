using AutoMapper;
using Core.DTO.Projects;
using Core.Entities;

namespace TaskManagementAPI.Mapping
{
    public class ProjectMapping : Profile
    {
        public ProjectMapping()
        {
            // Project -> ProjectDetailsDto
            CreateMap<Project, ProjectDetailsDto>()
                .ForMember(dest => dest.Owner,
                    opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.Members,
                    opt => opt.MapFrom(src => src.Members))
                .ForMember(dest => dest.IsOwner,
                    opt => opt.Ignore());

            // AppUser -> UserDto
            CreateMap<AppUser, UserDto>();

            // ProjectMember -> ProjectMemberDto
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}
