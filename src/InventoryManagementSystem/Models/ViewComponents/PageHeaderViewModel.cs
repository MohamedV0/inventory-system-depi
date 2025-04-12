using System;

namespace InventoryManagementSystem.Models.ViewComponents
{
    public class PageHeaderViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string IconClass { get; set; }
        public bool ShowRefreshButton { get; set; } = false;
        public string Description { get; set; }
        
        public string PrimaryButtonText { get; set; }
        public string PrimaryButtonUrl { get; set; }
        public string PrimaryButtonIcon { get; set; }
        
        public string SecondaryButtonText { get; set; }
        public string SecondaryButtonUrl { get; set; }
        public string SecondaryButtonIcon { get; set; }
        
        public bool IsSecondaryButtonDanger { get; set; } = false;
        public bool IsDeleteButton { get; set; } = false;
        public string EntityType { get; set; }
        public string EntityName { get; set; }
        public string DeleteFormId { get; set; }

        public static PageHeaderViewModel Create(string title, string iconClass = null)
        {
            return new PageHeaderViewModel
            {
                Title = title,
                IconClass = iconClass ?? "fas fa-list"
            };
        }
        
        public PageHeaderViewModel WithSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }
        
        public PageHeaderViewModel WithDescription(string description)
        {
            Description = description;
            return this;
        }
        
        public PageHeaderViewModel WithRefreshButton()
        {
            ShowRefreshButton = true;
            return this;
        }
        
        public PageHeaderViewModel WithPrimaryButton(string text, string url, string icon = null)
        {
            PrimaryButtonText = text;
            PrimaryButtonUrl = url;
            PrimaryButtonIcon = icon ?? "fas fa-plus";
            return this;
        }
        
        public PageHeaderViewModel WithSecondaryButton(string text, string url, string icon = null)
        {
            SecondaryButtonText = text;
            SecondaryButtonUrl = url;
            SecondaryButtonIcon = icon ?? "fas fa-arrow-left";
            IsSecondaryButtonDanger = false;
            return this;
        }
        
        public PageHeaderViewModel WithDangerButton(string text, string url, string icon = null)
        {
            SecondaryButtonText = text;
            SecondaryButtonUrl = url;
            SecondaryButtonIcon = icon ?? "fas fa-trash";
            IsSecondaryButtonDanger = true;
            return this;
        }

        public PageHeaderViewModel WithDeleteButton(string entityType, string entityName, string formId)
        {
            SecondaryButtonText = "Delete";
            SecondaryButtonIcon = "fas fa-trash";
            IsSecondaryButtonDanger = true;
            IsDeleteButton = true;
            EntityType = entityType;
            EntityName = entityName;
            DeleteFormId = formId;
            return this;
        }
    }
} 