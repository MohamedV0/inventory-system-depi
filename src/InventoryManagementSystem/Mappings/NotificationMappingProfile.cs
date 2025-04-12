using AutoMapper;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels.Notification;

namespace InventoryManagementSystem.Mappings
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notification, NotificationViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
        }
    }
} 