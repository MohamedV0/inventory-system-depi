using AutoMapper;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Mappings
{
    public class StockHistoryMappingProfile : Profile
    {
        public StockHistoryMappingProfile()
        {
            // StockHistory to StockHistoryViewModel mapping
            CreateMap<StockHistory, StockHistoryViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))
                .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.ReferenceNumber))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.CreatedBy));

            // StockHistory to StockHistoryDetailViewModel mapping
            CreateMap<StockHistory, StockHistoryDetailViewModel>()
                .IncludeBase<StockHistory, StockHistoryViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => 
                    src.Product != null && src.Product.Category != null ? src.Product.Category.Name : "Unknown"))
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.SKU : "Unknown"))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UpdatedBy));

            // AddStockViewModel to StockHistory mapping
            CreateMap<AddStockViewModel, StockHistory>()
                .ForMember(dest => dest.ReferenceNumber, opt => opt.MapFrom(src => src.Reference))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Quantity > 0 ? TransactionType.StockIn : TransactionType.StockOut))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousStock, opt => opt.Ignore())
                .ForMember(dest => dest.NewStock, opt => opt.Ignore());
        }
    }
} 