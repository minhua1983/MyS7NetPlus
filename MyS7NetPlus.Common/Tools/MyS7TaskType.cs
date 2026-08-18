using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tools
{
    public enum MyS7TaskType
    {
        ReadTagsFromMemory = 0,
        ReadAsync = 1,
        WriteAsync= 2,
        ReadBytesAsync = 3
    }
}
