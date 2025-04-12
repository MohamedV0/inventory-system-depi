using AutoMapper;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Entity to ViewModel mappings
            CreateMap<Product, ProductListItemViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"))
                .ForMember(dest => dest.NeedsReorder, opt => opt.MapFrom(src => src.CurrentStock <= src.ReorderLevel));

            CreateMap<Product, ProductDetailsViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"))
                .ForMember(dest => dest.Suppliers, opt => opt.MapFrom(src => src.ProductSuppliers))
                .ForMember(dest => dest.StockHistory, opt => opt.MapFrom(src => src.StockHistory));

            // ViewModel to Entity mappings
            CreateMap<CreateProductViewModel, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.StockHistory, opt => opt.Ignore());

            CreateMap<UpdateProductViewModel, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSuppliers, opt => opt.Ignore())
                .ForMember(dest => dest.StockHistory, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentStock, opt => opt.Condition((src, dest, srcMember) => src.CurrentStock > 0));

            // Additional product-related mappings
            CreateMap<ProductSupplier, ProductSupplierViewModel>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"))
                .ForMember(dest => dest.SupplierEmail, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Email : ""))
                .ForMember(dest => dest.IsPreferredSupplier, opt => opt.MapFrom(src => src.IsPreferred));

            CreateMap<StockHistory, StockHistoryViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.CreatedBy));
        }
    }
} 