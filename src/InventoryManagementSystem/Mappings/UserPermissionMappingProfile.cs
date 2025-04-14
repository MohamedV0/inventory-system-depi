using AutoMapper;
using InventoryManagementSystem.Models.Identity;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Mappings
{
    /// <summary>
    /// Mapping profile for user permissions
    /// </summary>
    public class UserPermissionMappingProfile : Profile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserPermissionMappingProfile()
        {
            CreateMap<Permission, UserPermissionViewModel>()
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.IsGranted, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
                
            CreateMap<UserPermission, UserPermissionViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionId))
                .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Permission.Description))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Permission.Category))
                .ForMember(dest => dest.IsGranted, opt => opt.MapFrom(src => src.IsGranted));
                
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());
        }
    }
} 