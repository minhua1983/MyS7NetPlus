using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tools
{
    public interface IMyMessageCallback
    {
        string Source { get; set; }
        SynchronizationContext SynchronizationContext { get; set; }
    }

    public class MyMessageCallback<T> : IMyMessageCallback
    {
        public string Source { get; set; }
        public SynchronizationContext? SynchronizationContext { get; set; }
        public Action<T> Callback { get; set; }
    }
}
