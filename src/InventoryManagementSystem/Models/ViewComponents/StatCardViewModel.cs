using Microsoft.AspNetCore.Html;

namespace InventoryManagementSystem.Models.ViewComponents
{
    public class StatCardViewModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public string Icon { get; set; }
        public string IconClass { get; set; }
        
        // Trend support
        public bool ShowTrend { get; set; }
        public decimal TrendValue { get; set; }
        public string TrendLabel { get; set; } = "vs last month";
        
        // Additional content support
        public IHtmlContent AdditionalContent { get; set; }
        
        // Fluent API for easy construction
        public static StatCardViewModel Create(string title, string value, string icon)
        {
            return new StatCardViewModel
            {
                Title = title,
                Value = value,
                Icon = icon,
                IconClass = "bg-primary-subtle",
                ShowTrend = false
            };
        }
        
        public StatCardViewModel WithIconClass(string iconClass)
        {
            IconClass = iconClass;
            return this;
        }
        
        public StatCardViewModel WithSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }
        
        public StatCardViewModel WithTrend(decimal trendValue, string trendLabel = null)
        {
            ShowTrend = true;
            TrendValue = trendValue;
            if (trendLabel != null) TrendLabel = trendLabel;
            return this;
        }
        
        public StatCardViewModel WithAdditionalContent(IHtmlContent content)
        {
            AdditionalContent = content;
            return this;
        }
    }
} 