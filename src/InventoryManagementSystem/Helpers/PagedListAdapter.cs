using X.PagedList.Mvc.Core;
using X.PagedList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryManagementSystem.Helpers
{
    /// <summary>
    /// Helper class for X.PagedList pagination configuration and extensions
    /// </summary>
    public static class PagedListAdapter
    {
        /// <summary>
        /// Maps entities in a paged list to another type while preserving pagination metadata
        /// </summary>
        /// <typeparam name="TSource">The source type in the paged list</typeparam>
        /// <typeparam name="TDestination">The destination type for the new paged list</typeparam>
        /// <param name="source">The source paged list</param>
        /// <param name="mapper">A function that maps from TSource to TDestination</param>
        /// <returns>A new paged list with mapped items but the same pagination metadata</returns>
        public static IPagedList<TDestination> MapPagedList<TSource, TDestination>(
            this IPagedList<TSource>? source, 
            Func<TSource, TDestination> mapper)
        {
            if (source == null)
            {
                // Return an empty paged list
                return new StaticPagedList<TDestination>(
                    new List<TDestination>(),
                    1, // Page number
                    10, // Page size
                    0 // Total count
                );
            }
            
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var items = source.Select(mapper).ToList();
            return new StaticPagedList<TDestination>(
                items, 
                source.PageNumber, 
                source.PageSize, 
                source.TotalItemCount
            );
        }
        
        /// <summary>
        /// Get standardized PagedList options for consistent pagination UI across the application
        /// </summary>
        /// <returns>PagedListRenderOptions with consistent styling and functionality</returns>
        public static PagedListRenderOptions GetStandardOptions()
        {
            return new PagedListRenderOptions
            {
                LiElementClasses = new string[] { "page-item" },
                PageClasses = new string[] { "page-link" },
                DisplayLinkToFirstPage = PagedListDisplayMode.Always,
                DisplayLinkToLastPage = PagedListDisplayMode.Always,
                DisplayLinkToPreviousPage = PagedListDisplayMode.Always,
                DisplayLinkToNextPage = PagedListDisplayMode.Always,
                DisplayLinkToIndividualPages = true,
                DisplayEllipsesWhenNotShowingAllPageNumbers = true,
                MaximumPageNumbersToDisplay = 5,
                EllipsesFormat = "...",
                LinkToFirstPageFormat = "«",
                LinkToPreviousPageFormat = "Previous",
                LinkToNextPageFormat = "Next",
                LinkToLastPageFormat = "»",
                ContainerDivClasses = new[] { "pagination-container" },
                UlElementClasses = new[] { "pagination", "justify-content-center" },
                ClassToApplyToFirstListItemInPager = null,
                ClassToApplyToLastListItemInPager = null,
                ActiveLiElementClass = "active"
            };
        }

        /// <summary>
        /// Creates pagination URL with common sorting and filtering parameters
        /// </summary>
        public static Dictionary<string, object> GetPageUrlParams(
            int page, 
            string? searchTerm = null, 
            string? sortBy = null, 
            bool? ascending = null, 
            int? categoryId = null,
            int? supplierId = null,
            int? productId = null,
            int? transactionType = null,
            int? pageSize = null)
        {
            var parameters = new Dictionary<string, object> { { "page", page } };
            
            if (pageSize.HasValue)
                parameters.Add("pageSize", pageSize.Value);
                
            if (!string.IsNullOrEmpty(searchTerm))
                parameters.Add("searchTerm", searchTerm);
                
            if (!string.IsNullOrEmpty(sortBy))
                parameters.Add("sortBy", sortBy);
                
            if (ascending.HasValue)
                parameters.Add("ascending", ascending.Value);
                
            if (categoryId.HasValue)
                parameters.Add("categoryId", categoryId.Value);
                
            if (supplierId.HasValue)
                parameters.Add("supplierId", supplierId.Value);
                
            if (productId.HasValue)
                parameters.Add("productId", productId.Value);
                
            if (transactionType.HasValue)
                parameters.Add("transactionType", transactionType.Value);
                
            return parameters;
        }
    }
} 