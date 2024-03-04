using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace Chat.Shared
{
    public class PaketBuilder 
    {
        private MemoryStream memoryStream;

        public byte[] BuildMessage(PaketContainer container)
        {
            return BuildMessage(container.OperationCode, container.Payload.ToArray());
        }

        public byte[] BuildMessage(NetworkOperationCode code, string message)
        {
            memoryStream = new MemoryStream();
            WriteOpCode(code);
            WriteMessageLength(1);
            WriteString(message);
            return GetPaketBytes();
        }

        public byte[] BuildMessage(NetworkOperationCode code, params string[] message)
        {
            memoryStream = new MemoryStream();
            WriteOpCode(code);
            WriteMessageLength(message.Length);
            foreach (string s in message)
            {
                WriteString(s);
            }
            return GetPaketBytes();
        }

        private void WriteMessageLength(int length)
        {
            byte lengthByte = Convert.ToByte(length);
            memoryStream.WriteByte(lengthByte);
        }

        private void WriteOpCode(NetworkOperationCode opCode)
        {
            byte opCodeByte = (byte)opCode;
            memoryStream.WriteByte(opCodeByte);
        }

        private void WriteString(string str)
        {
            //WriteMessage func in PacketBuilder class, ms.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length); (not buff.Length)
            memoryStream.Write(BitConverter.GetBytes(str.Length));
            //ToDo, Try convert to UTF8
            memoryStream.Write(Encoding.ASCII.GetBytes(str));
        }

        private byte[] GetPaketBytes()
        {
            return memoryStream.ToArray();
        }
    }
}
