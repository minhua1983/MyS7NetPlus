using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tool
{
    public class MyPersistance
    {
        public MyPersistanceType MyPersistanceType { get; set; }
        public object State { get; set; }

        public Action Callback { get; set; }
    }
}
