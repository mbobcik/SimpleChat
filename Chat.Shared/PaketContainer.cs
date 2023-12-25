using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public class PaketContainer
    {
        public NetworkOperationCode OperationCode { get; set; }
        public List<string> Payload { get; set; } = new List<string>();
    }
}
