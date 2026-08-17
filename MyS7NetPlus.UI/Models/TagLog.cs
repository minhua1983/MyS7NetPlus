
namespace MyS7NetPlus.UI.Models
{
    public class TagLog
    {
        public int Id { get; set; }
        public string? DeviceName { get; set; }
        public string? GroupName { get; set; }
        public string? TagName { get; set; }
        public string? TagValue { get; set; }
        public long CollectedAt { get; set; } 
    }
}
