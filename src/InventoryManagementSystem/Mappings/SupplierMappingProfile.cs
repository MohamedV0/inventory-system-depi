using AutoMapper;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Mappings
{
    public class SupplierMappingProfile : Profile
    {
        public SupplierMappingProfile()
        {
            // Entity to ViewModel mappings
            CreateMap<Supplier, SupplierListItemViewModel>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.ProductSuppliers != null ? src.ProductSuppliers.Count : 0));

            CreateMap<Supplier, SupplierViewModel>();

            CreateMap<Supplier, SupplierDetailsViewModel>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.ProductSuppliers))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.ProductSuppliers != null ? src.ProductSuppliers.Count : 0));

            // ViewModel to Entity mappings
            CreateMap<CreateSupplierViewModel, Supplier>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore());

            CreateMap<UpdateSupplierViewModel, Supplier>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore());

            // ProductSupplier to SupplierProductViewModel
            CreateMap<ProductSupplier, SupplierProductViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))
                .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : "Unknown"));
        }
    }
} 