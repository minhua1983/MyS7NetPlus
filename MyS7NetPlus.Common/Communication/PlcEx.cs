using S7.Net;
using S7.Net.Types;
using System.Collections;
using System.Text.RegularExpressions;

namespace MyS7NetPlus.Common.Communication
{
    public class TypeMapping
    {
        public VarType MappingVarType { get; set; }
        public required string MatchingPattern { get; set; }
        public required byte NeedByteCount { get; set; }
    }
    public static class PlcEx
    {
        public static readonly Dictionary<Type, TypeMapping> TypeMappingList = new Dictionary<Type, TypeMapping>()
        {
            { typeof(bool), new TypeMapping() { MappingVarType = VarType.Bit, NeedByteCount = 1, MatchingPattern = "^(I|Q|M|DB\\d+\\.DBX)\\d+\\.\\d+$" } },
            { typeof(sbyte), new TypeMapping() { MappingVarType = VarType.Byte, NeedByteCount = 1, MatchingPattern = "^(IB|QB|MB|DB\\d+\\.DBB)\\d+$" } },
            { typeof(byte), new TypeMapping() { MappingVarType = VarType.Byte, NeedByteCount = 1, MatchingPattern = "^(IB|QB|MB|DB\\d+\\.DBB)\\d+$" } },
            { typeof(short), new TypeMapping() { MappingVarType = VarType.Int, NeedByteCount = 2, MatchingPattern = "^(IW|QW|MW|DB\\d+\\.DBW)\\d+$" } },
            { typeof(ushort), new TypeMapping() { MappingVarType = VarType.Word, NeedByteCount = 2, MatchingPattern = "^(IW|QW|MW|DB\\d+\\.DBW)\\d+$" } },
            { typeof(int), new TypeMapping() { MappingVarType = VarType.DInt, NeedByteCount = 4, MatchingPattern = "^(ID|QD|MD|DB\\d+\\.DBD)\\d+$" } },
            { typeof(uint), new TypeMapping() { MappingVarType = VarType.DWord, NeedByteCount = 4, MatchingPattern = "^(ID|QD|MD|DB\\d+\\.DBD)\\d+$" } },
            { typeof(float), new TypeMapping() { MappingVarType = VarType.Real, NeedByteCount = 4, MatchingPattern = "^(ID|QD|MD|DB\\d+\\.DBD)\\d+$" } },
        };

        /// <summary>
        /// 本泛型方法目前只支持I，Q，M，DB区读取相关数据，不支持LInt/ULInt/LReal类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<T[]> ReadAsync<T>(this Plc instance, string address, ushort count) where T : struct
        {
            /*
            只支持I，Q，M，DB区

            所有格式如下：
                I0.0
                IB0
                IW0
                ID0

                Q0.0
                QB0
                QW0
                QD0

                M0.0
                MB0
                MW0
                MD0

                DB1.DBX0.0
                DB1.DBB0
                DB1.DBW0
                DB1.DBD0
            
            T:bool类型 -> siemens Bool，占1个bit
                I0.0
                Q0.0
                M0.0
                DB1.DBX0.0

            T:sbyte/byte类型 -> siemens SInt/USint，占1个byte
                IB0
                QB0
                MB0
                DB1.DBB0

            T:short/ushort类型 -> siemens Int/UInt，占2个byte
                IW0
                QW0
                MW0
                DB1.DBW0

            T:int/uint/float类型 -> siemens DInt/UDInt/Real，占4个byte
                ID0
                QD0
                MD0
                DB1.DBD0

            T:long/ulong/double类型 -> siemens LInt/ULInt/LReal，占8个byte，暂时不支持，因为寻址表达式不支持识别8字节。
                如有需要，请使用Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)方法获取LReal的值，使用ReadBytes(DataType dataType, int db, int startByteAdr, int count)方法获取LInt/ULInt的值
                或者自己写类似IL0，QL0，ML0，DB1.DBL0来寻址，其内部调用上面2个方法来获取LInt/ULInt/LReal的值。但是这种做法不友好，容易让新手以为siemens原生支持L这种8位寻址表达式。
            //*/
            T[] v = new T[count];

            address = address.Trim();

            // 判断T是否符合指定类型
            if (!TypeMappingList.Keys.Contains(typeof(T)))
            {
                throw new Exception($"type:{typeof(T).Name} is not supported yet");
            }

            // 判断寻址表达式是否和T相匹配
            if (!Regex.IsMatch(address, TypeMappingList[typeof(T)].MatchingPattern, RegexOptions.IgnoreCase))
            {
                throw new Exception($"type:{typeof(T).Name} is not matched with pattern:{TypeMappingList[typeof(T)].MatchingPattern}");
            }

            MyAddress myAddress = new(address);

            if (typeof(T) == typeof(bool))
            {
                // for bool ReadAsync
                var totalBitOffset = myAddress.ByteOffset * 8 + myAddress.BitOffset;

                var bools = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    var currentTotalBitOffset = totalBitOffset + i;
                    var currentByteOffset = currentTotalBitOffset / 8;
                    var currentBitOffset = (byte)(currentTotalBitOffset % 8);

                    bools[i] = (bool)(await instance.ReadAsync(myAddress.DataType, myAddress.DbIndex, currentByteOffset, TypeMappingList[typeof(T)].MappingVarType, 1, currentBitOffset))!;
                }
                v = (T[])(object)bools;
            }
            else
            {
                // for other ReadBytesAsync
                var bytes = await instance.ReadBytesAsync(myAddress.DataType, myAddress.DbIndex, myAddress.ByteOffset, TypeMappingList[typeof(T)].NeedByteCount * count);

                for (int i = 0; i < v.Length; i++)
                {
                    var tempBytes = bytes.Skip(TypeMappingList[typeof(T)].NeedByteCount * i).Take(TypeMappingList[typeof(T)].NeedByteCount).ToArray();
                    v[i] = ToValue<T>(tempBytes);
                }
            }

            return v;
        }


        public static T ToValue<T>(byte[] bytes) where T : struct
        {
            Array.Reverse(bytes);

            Type type = typeof(T);

            // 1. 单字节（byte / sbyte）
            if (type == typeof(byte)) return (T)(object)bytes[0];
            if (type == typeof(sbyte)) return (T)(object)unchecked((sbyte)bytes[0]);

            // 2. 双字节（short / ushort）
            if (type == typeof(short)) return (T)(object)BitConverter.ToInt16(bytes, 0);
            if (type == typeof(ushort)) return (T)(object)BitConverter.ToUInt16(bytes, 0);

            // 3. 四字节（int / uint / float）
            if (type == typeof(int)) return (T)(object)BitConverter.ToInt32(bytes, 0);
            if (type == typeof(uint)) return (T)(object)BitConverter.ToUInt32(bytes, 0);
            if (type == typeof(float)) return (T)(object)BitConverter.ToSingle(bytes, 0);

            /*
            // 4. 八字节（long / ulong / double）
            if (type == typeof(long)) return (T)(object)BitConverter.ToInt64(bytes, 0);
            if (type == typeof(ulong)) return (T)(object)BitConverter.ToUInt64(bytes, 0);
            if (type == typeof(double)) return (T)(object)BitConverter.ToDouble(bytes, 0);
            //*/

            throw new NotSupportedException($"不支持的类型: {type.Name}");
        }

        public static async Task WriteAsync<T>(this Plc instance, string address, T[] values) where T : struct
        {
            /*
            只支持I，Q，M，DB区

            所有格式如下：
                I0.0
                IB0
                IW0
                ID0

                Q0.0
                QB0
                QW0
                QD0

                M0.0
                MB0
                MW0
                MD0

                DB1.DBX0.0
                DB1.DBB0
                DB1.DBW0
                DB1.DBD0
            
            T:bool类型 -> siemens Bool，占1个bit
                I0.0
                Q0.0
                M0.0
                DB1.DBX0.0

            T:sbyte/byte类型 -> siemens SInt/USint，占1个byte
                IB0
                QB0
                MB0
                DB1.DBB0

            T:short/ushort类型 -> siemens Int/UInt，占2个byte
                IW0
                QW0
                MW0
                DB1.DBW0

            T:int/uint/float类型 -> siemens DInt/UDInt/Real，占4个byte
                ID0
                QD0
                MD0
                DB1.DBD0

            T:long/ulong/double类型 -> siemens LInt/ULInt/LReal，占8个byte，暂时不支持，因为寻址表达式不支持识别8字节。
                如有需要，请使用Write(DataType dataType, int db, int startByteAdr, object value)方法写入LReal的值，使用WriteBytes方法写入LInt/ULInt的值
                或者自己写类似IL0，QL0，ML0，DB1.DBL0来寻址，其内部调用上面2个方法来获取LInt/ULInt/LReal的值。但是这种做法不友好，容易让新手以为siemens原生支持L这种8位寻址表达式。
            //*/
            if (values == null) throw new Exception($"values:{values} cannot be null");

            address = address.Trim();

            // 判断T是否符合指定类型
            if (!TypeMappingList.Keys.Contains(typeof(T)))
            {
                throw new Exception($"type:{typeof(T).Name} is not supported yet");
            }

            // 判断寻址表达式是否和T相匹配
            if (!Regex.IsMatch(address, TypeMappingList[typeof(T)].MatchingPattern, RegexOptions.IgnoreCase))
            {
                throw new Exception($"type:{typeof(T).Name} is not matched with pattern:{TypeMappingList[typeof(T)].MatchingPattern}");
            }

            MyAddress myAddress = new(address);

            if (typeof(T) == typeof(bool))
            {
                // for bool WriteAsync
                var totalBitOffset = myAddress.ByteOffset * 8 + myAddress.BitOffset;
                List<DataItem> dataItems = new();
                for (int i = 0; i < values.Length; i++)
                {
                    var currentTotalBitOffset = totalBitOffset + i;
                    var currentByteOffset = currentTotalBitOffset / 8;
                    var currentBitOffset = (byte)(currentTotalBitOffset % 8);

                    await instance.WriteAsync(myAddress.DataType, myAddress.DbIndex, currentByteOffset, values[i]!, currentBitOffset);
                }
            }
            else
            {
                // for other WriteBitesAsync
                //await instance.WriteAsync(dataType, dbIndex, byteOffset, values, bitOffset);
                byte[] bytes = new byte[TypeMappingList[typeof(T)].NeedByteCount * values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    var tempBytes = ToBytes<T>(values[i]);
                    for (int j = 0; j < TypeMappingList[typeof(T)].NeedByteCount; j++)
                    {
                        bytes[TypeMappingList[typeof(T)].NeedByteCount * i + j] = tempBytes[j];
                    }
                }
                await instance.WriteBytesAsync(myAddress.DataType, myAddress.DbIndex, myAddress.ByteOffset, bytes);
            }
        }

        public static byte[] ToBytes<T>(T value) where T : struct
        {
            Type type = typeof(T);
            byte[] bytes;
            if (type == typeof(byte)) bytes = new byte[] { (byte)(object)value };
            else if (type == typeof(sbyte)) bytes = new byte[] { unchecked((byte)(sbyte)(object)value) };
            else if (type == typeof(short)) bytes = BitConverter.GetBytes((short)(object)value);
            else if (type == typeof(ushort)) bytes = BitConverter.GetBytes((ushort)(object)value);
            else if (type == typeof(int)) bytes = BitConverter.GetBytes((int)(object)value);
            else if (type == typeof(uint)) bytes = BitConverter.GetBytes((uint)(object)value);
            else if (type == typeof(float)) bytes = BitConverter.GetBytes((float)(object)value);
            //else if (type == typeof(long)) bytes = BitConverter.GetBytes((long)(object)value);
            //else if (type == typeof(ulong)) bytes = BitConverter.GetBytes((ulong)(object)value);
            //else if (type == typeof(double)) bytes = BitConverter.GetBytes((double)(object)value);
            //// 还可以加bool、char等
            //else if (type == typeof(bool)) bytes = BitConverter.GetBytes((bool)(object)value);
            else throw new NotImplementedException($"不支持的类型：{type.Name}");

            // 如果需要统一为小端序，且系统是大端，则反转
            //if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            Array.Reverse(bytes);
            return bytes;
        }
    }
}



