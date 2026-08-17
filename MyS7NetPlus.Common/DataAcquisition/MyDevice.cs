using MyS7NetPlus.Common.Communication;
using MyS7NetPlus.Common.Tool;
using Newtonsoft.Json;

namespace MyS7NetPlus.Common.DataAcquisition
{
    public class MyDevice
    {
        [JsonIgnore]
        public MyS7Context MyS7Context { get; set; }
        public byte Id { get; set; } = 0x01;
        public string Name { get; set; }
        public string Description { get; set; }
        public MyProtocol Protocol { get; set; } = MyProtocol.SiemensS7;
        public string IpAddress { get; set; }
        public ushort Port { get; set; }
        public List<MyGroup> GroupList { get; set; } = new();

    }
}
