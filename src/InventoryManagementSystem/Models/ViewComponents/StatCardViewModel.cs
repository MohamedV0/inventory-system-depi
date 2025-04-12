namespace InventoryManagementSystem.Models.ViewComponents
{
    public class StatCardViewModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Subtext { get; set; }
        public string IconClass { get; set; }
        
        public static StatCardViewModel Create(string label, string value, string icon)
        {
            return new StatCardViewModel
            {
                Label = label,
                Value = value,
                IconClass = icon
            };
        }
        
        public StatCardViewModel WithSubtext(string subtext)
        {
            Subtext = subtext;
            return this;
        }
    }
} 