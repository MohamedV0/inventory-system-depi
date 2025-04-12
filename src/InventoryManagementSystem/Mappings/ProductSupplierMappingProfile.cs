using AutoMapper;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Mappings
{
    public class ProductSupplierMappingProfile : Profile
    {
        public ProductSupplierMappingProfile()
        {
            // Entity to ViewModel mappings
            CreateMap<ProductSupplier, ProductSupplierListItemViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name))
                .ForMember(dest => dest.IsPreferredSupplier, opt => opt.MapFrom(src => src.IsPreferredSupplier));

            CreateMap<ProductSupplier, ProductSupplierDetailsViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
                .ForMember(dest => dest.SupplierContactName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.ContactPerson : string.Empty))
                .ForMember(dest => dest.SupplierEmail, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Email : string.Empty))
                .ForMember(dest => dest.SupplierPhone, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Phone : string.Empty))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product != null ? new ProductBasicViewModel
                {
                    Id = src.Product.Id,
                    Name = src.Product.Name,
                    SKU = src.Product.SKU
                } : null))
                .ForMember(dest => dest.Supplier, opt => opt.MapFrom(src => src.Supplier != null ? new SupplierBasicViewModel
                {
                    Id = src.Supplier.Id,
                    Name = src.Supplier.Name,
                    Email = src.Supplier.Email
                } : null));

            // ViewModel to Entity mappings
            CreateMap<CreateProductSupplierViewModel, ProductSupplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            CreateMap<UpdateProductSupplierViewModel, ProductSupplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Map supplier to SupplierDetailsViewModel
            CreateMap<Supplier, SupplierDetailsViewModel>();
        }
    }
} 