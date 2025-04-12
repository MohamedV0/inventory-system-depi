using AutoMapper;
using X.PagedList;

namespace InventoryManagementSystem.Extensions
{
    public static class AutoMapperExtensions
    {
        /// <summary>
        /// Maps a paged list from source to destination type
        /// </summary>
        public static IPagedList<TDestination> ToMappedPagedList<TSource, TDestination>(
            this IPagedList<TSource> list, 
            IMapper mapper)
        {
            var mappedItems = mapper.Map<IEnumerable<TDestination>>(list);
            return new StaticPagedList<TDestination>(
                mappedItems, 
                list.GetMetaData().PageNumber, 
                list.GetMetaData().PageSize, 
                list.GetMetaData().TotalItemCount);
        }

        /// <summary>
        /// Projects a queryable source to a queryable destination
        /// for more efficient mapping of EF Core queries
        /// </summary>
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source, 
            IMapper mapper)
        {
            return mapper.ProjectTo<TDestination>(source);
        }
    }
} 