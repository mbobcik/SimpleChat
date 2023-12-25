using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public enum NetworkOperationCode
    {
        None = 0,
        NewConnection,
        NewClientBroadcast
    }
}
