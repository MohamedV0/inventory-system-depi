using System;
using System.Linq.Expressions;
using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Data.Specifications
{
    public class BaseSupplierSpecification : BaseSpecification<Supplier>
    {
        public BaseSupplierSpecification()
        {
            // Base specification with no criteria
        }

        public BaseSupplierSpecification(Expression<Func<Supplier, bool>> criteria) : base(criteria)
        {
        }

        public BaseSupplierSpecification IncludeProductSuppliers()
        {
            AddInclude(s => s.ProductSuppliers);
            return this;
        }

        public BaseSupplierSpecification IncludeProducts()
        {
            AddInclude("ProductSuppliers.Product");
            return this;
        }

        public BaseSupplierSpecification OrderByName()
        {
            ApplyOrderBy(s => s.Name);
            return this;
        }

        public BaseSupplierSpecification OrderByNameDescending()
        {
            ApplyOrderByDescending(s => s.Name);
            return this;
        }

        public BaseSupplierSpecification WithActiveOnly()
        {
            Criteria = s => s.IsActive;
            return this;
        }
    }

    public class SupplierWithProductsSpecification : BaseSupplierSpecification
    {
        public SupplierWithProductsSpecification() : base()
        {
            AddInclude(s => s.ProductSuppliers);
            AddInclude("ProductSuppliers.Product");
        }

        public SupplierWithProductsSpecification(int supplierId) 
            : base(s => s.Id == supplierId)
        {
            AddInclude(s => s.ProductSuppliers);
            AddInclude("ProductSuppliers.Product");
        }
    }
} 