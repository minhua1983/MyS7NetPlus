using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tools
{
    public class MyLogEventArgs : EventArgs
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
}
