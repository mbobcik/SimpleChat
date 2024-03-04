using Chat.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class ServerConfiguration : ConfigurationBase
    {
        public int Port { get; set; }

        public ServerConfiguration() 
        {
            this.Port = 7981;
        }
    }
}
