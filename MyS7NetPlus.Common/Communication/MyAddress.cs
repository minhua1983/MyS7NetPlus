using S7.Net;
using System.Text.RegularExpressions;

namespace MyS7NetPlus.Common.Communication
{
    public class MyAddress
    {
        public MyAddress(string startAddress)
        {
            StartAddress = startAddress;

            // 从寻址表达式+T拆解DataType，DB号，地址偏移，VarType，字节数量，位所在索引(0-7)
            string pattern = @"^(?:(?<Area>I|Q|M)(?:(?<Type>[BWD])(?<Offset>\d+)|(?<Offset>\d+)\.(?<Bit>\d+))|(?<Area>DB)(?<DbIndex>\d+)\.DB(?<Type>[BWDX])(?<Offset>\d+)(?:\.(?<Bit>\d+))?)$";

            var groups = Regex.Match(StartAddress, pattern).Groups;
            var area = groups["Area"].Value;
            DbIndex = groups["DbIndex"].Value == string.Empty ? (ushort)0 : ushort.Parse(groups["DbIndex"].Value);
            ByteOffset = ushort.Parse(groups["Offset"].Value);
            BitOffset = groups["Bit"].Value == string.Empty ? (byte)0 : byte.Parse(groups["Bit"].Value);

            DataType = area.ToUpper() switch
            {
                "I" => DataType.Input,
                "Q" => DataType.Output,
                "M" => DataType.Memory,
                "DB" => DataType.DataBlock,
                _ => throw new Exception($"area:{area} is not supported yet")
            };

            ByteCount = groups["Type"].Value.ToUpper() switch
            {
                "X" => 1,
                "B" => 1,
                "W" => 2,
                "D" => 4,
                _ => 1
            };
        }

        public string StartAddress { get; set; }
        public DataType DataType { get; set; }
        public ushort DbIndex { get; set; } = 1;
        public ushort ByteOffset { get; set; } = 0;
        public byte BitOffset { get; set; } = 0;
        public ushort ByteCount { get; set; } = 1;
    }
}
