using Newtonsoft.Json;
using S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.DataAcquisitions
{
    public class MyTag : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        object _value;

        [JsonIgnore]
        public MyGroup MyGroup { get; set; }
        public byte Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public string StartAddress { get; set; }
        public DataType DataType { get; set; }
        public ushort DbIndex { get; set; } = 1;
        public ushort ByteOffset { get; set; } = 0;
        public byte BitOffset { get; set; } = 0;
        public ushort ByteCount { get; set; } = 1;
        public string ValueType { get; set; }
        public decimal Scale { get; set; } = 1.0M;
        public bool NeedToMonitor { get; set; } = false;
        public bool BooleanThreshold { get; set; }
        public object HighThreshold { get; set; }
        public object HighDeadBand { get; set; }
        public object LowThreshold { get; set; }
        public object LowDeadBand { get; set; }
        [JsonIgnore]
        public bool IsNoticed { get; set; } = false;
        [JsonIgnore]
        public DateTime LastNoticed { get; set; } = DateTime.UtcNow;
        public int OnDelay { get; set; } = 3000;
        [JsonIgnore]
        public bool IsAlarmed { get; set; } = false;
        [JsonIgnore]
        public DateTime LastAlarmed { get; set; } = DateTime.UtcNow;
        public int OffDelay { get; set; } = 3000;
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}