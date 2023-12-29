using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public class PaketReader : BinaryReader
    {
        private NetworkStream stream;
        public PaketReader(NetworkStream stream) : base(stream)
        {
            this.stream = stream;
        }

        public PaketContainer ReadMessage()
        {
            var opCode = ReadOpCode();
            var payload = ReadPayload();
            var message = new PaketContainer 
            {
                OperationCode = opCode,
                Payload = payload
            };
            var log = string.Join("}, {", message.Payload.ToArray());
            Console.WriteLine($"Got message with OpCode {opCode}; Payload {{{log}}}");
            return message;
        }

        private NetworkOperationCode ReadOpCode()
        {
            var opCodeByte = stream.ReadByte();
            return (NetworkOperationCode)opCodeByte;
        }

        private List<string> ReadPayload()
        {
            var payloadLength = stream.ReadByte();
            var payload = new List<string>();
            for (int i = 0;i < payloadLength; i++) // https://youtu.be/I-Xmp-mulz4?t=2317
            {
                byte[] buffer;
                // Read messageLength from message (NetworkStream)
                var messageLength = ReadInt32();
                buffer = new byte[messageLength];
                stream.Read(buffer, 0, messageLength);
                string message = Encoding.ASCII.GetString(buffer);

                if (!string.IsNullOrEmpty(message))
                {
                    payload.Add(message);
                }
            }
            return payload;
        }
    }
}
