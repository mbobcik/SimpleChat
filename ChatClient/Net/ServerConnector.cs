using Chat.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    internal class ServerConnector
    {
        TcpClient client;
        PaketBuilder paketBuilder;

        public event Action<PaketContainer>? ConnectedEvent;

        public ServerConnector()
        {
            client = new TcpClient();
            paketBuilder = new PaketBuilder();
        }

        public void Connect(string username)
        {
            if (!client.Connected)
            {
                client.Connect("127.0.0.1", 7981);
                PaketReader paketReader = new PaketReader(client.GetStream());
                if (!string.IsNullOrEmpty(username))
                {
                    var connectMessage = this.paketBuilder.BuildMessage(NetworkOperationCode.NewConnection, username);
                    client.Client.Send(connectMessage);
                }

                ReadPackets(paketReader);
            }
        }

        private void ReadPackets(PaketReader paketReader)
        {
            Task.Factory.StartNew(() =>
            {
                string log;
                while (true)
                {
                    var message = paketReader.ReadMessage();
                    switch (message.OperationCode)
                    {
                        case NetworkOperationCode.None:
                            break;
                        case NetworkOperationCode.NewConnection:
                            log = string.Join("}, {", message.Payload.ToArray());
                            Console.WriteLine($"WARN: Wrong OpCode in this state. OpCode {message.OperationCode}; Payload {{{log}}}");
                            break;
                        case NetworkOperationCode.NewClientBroadcast:
                            ConnectedEvent?.Invoke(message);
                            break;
                        default:
                            log = string.Join("}, {", message.Payload.ToArray());
                            Console.WriteLine($"This should not happen. OpCode {message.OperationCode}; Payload {{{log}}}");
                            break;
                    }
                }
            });
        }
    }
}
