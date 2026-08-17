using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tool
{
    public class MyEventArgs
    {
        object _state;
        public MyEventArgs(object state)
        {
            _state = state;
        }
        public object State
        {
            get { return _state; }
            set { _state = value; }
        }
    }
}
