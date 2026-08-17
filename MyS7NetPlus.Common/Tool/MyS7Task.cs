using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tool
{
    public class MyS7Task
    {
        public TaskCompletionSource<object> TaskCompletionSource { get; set; }
        public string IpAddress { get; set; }
        public MyS7TaskType MyS7TaskType { get; set; }
        public string ValueType { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string StartAddress { get; set; }
        //public DataType DataType { get; set; }
        //public ushort DbIndex { get; set; } = 1;
        //public ushort ByteOffset { get; set; } = 0;
        //public byte BitOffset { get; set; } = 0;
        public ushort ByteCount { get; set; } = 1;
    }
}
