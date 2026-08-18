using Newtonsoft.Json;
using S7.Net;
using System.ComponentModel;

namespace MyS7NetPlus.Common.DataAcquisitions
{
    public class MyGroup
    {
        [JsonIgnore]
        public MyDevice MyDevice { get; set; }
        public byte Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StartAddress { get; set; }
        public DataType DataType { get; set; }
        public ushort DbIndex { get; set; } = 1;
        public ushort ByteOffset { get; set; } = 0;
        public byte BitOffset { get; set; } = 0;
        public ushort ByteCount { get; set; } = 0;
        public BindingList<MyTag> TagList { get; set; } = new();
    }
}
