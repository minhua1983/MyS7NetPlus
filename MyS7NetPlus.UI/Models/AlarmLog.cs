using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.UI.Models
{
    public class AlarmLog
    {
        public int Id { get; set; }
        public string? DeviceName { get; set; }
        public string? GroupName { get; set; }
        public string? TagName { get; set; }
        public string? TagValue { get; set; }
        public bool IsNoticed { get; set; } = false;
        public bool IsAlarmed { get; set; } = false;
        public string? Message { get; set; }
        public long Duration { get; set; } = 0;
        public long TriggeredAt { get; set; }
    }
}
