using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public static class AddressHelper
    {
        public static bool ValidateServerAddress(string addressWithPort)
        {
            if (string.IsNullOrWhiteSpace(addressWithPort))
            {
                return false;
            }
            if (addressWithPort.Count(x => x == ':') != 1)
            {
                return false;
            }
            string address = ParseAddress(addressWithPort);
            if (ParsePort(addressWithPort) == -1)
            {
                return false;
            }
            if (Uri.CheckHostName(address) == UriHostNameType.Unknown)
            {
                return false;
            }

            return true;
        }

        public static string ParseAddress(string? addressWithPort)
        {
            if (!string.IsNullOrWhiteSpace(addressWithPort))
            {
                return addressWithPort[..addressWithPort.IndexOf(':')];
            }
            return String.Empty;
        }

        public static int ParsePort(string? addressWithPort)
        {
            if (string.IsNullOrWhiteSpace(addressWithPort))
            {
                return -1;
            }
            string portString = addressWithPort.Substring(addressWithPort.IndexOf(":") + 1);
            int port;
            if (!int.TryParse(portString, out port) || port > 65535 || port < 1)
            {
                return -1;
            }
            return port;
        }
    }
}
