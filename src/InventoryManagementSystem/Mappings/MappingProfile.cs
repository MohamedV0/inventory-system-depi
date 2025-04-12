using AutoMapper;

namespace InventoryManagementSystem.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Global mapping configurations
            CreateMap<string, string>().ConvertUsing(s => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim());
        }
    }
} 