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
        PaketBuilder packetBuilder;

        public event Action<PaketContainer>? ConnectedEvent;
        public event Action<PaketContainer>? MessageReceivedEvent;
        public event Action<PaketContainer>? DisconnectedUserEvent;

        public ServerConnector()
        {
            client = new TcpClient();
            packetBuilder = new PaketBuilder();
        }

        public async Task ConnectAsync(string username, string addressWithPort)
        {
            if (!client.Connected)
            {
                try
                {
                      await client.ConnectAsync(AddressHelper.ParseAddress(addressWithPort), AddressHelper.ParsePort(addressWithPort));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to connect to client with following exception: {ex}");
                }
                PaketReader paketReader = new PaketReader(client.GetStream());
                if (!string.IsNullOrEmpty(username))
                {
                    var connectMessage = this.packetBuilder.BuildMessage(NetworkOperationCode.NewConnection, username);
                    client.Client.Send(connectMessage);
                }

                ReadPackets(paketReader);
            }
        }

        public void SendMessageToServer(string message)
        {
            var sentAt = DateTime.Now.ToString();
            var messagePacket = this.packetBuilder.BuildMessage(NetworkOperationCode.MessageToServer, message, sentAt);
            this.client.Client.Send(messagePacket);
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
                        case NetworkOperationCode.MessageToServer:
                            this.MessageReceivedEvent?.Invoke(message);
                            break;
                        case NetworkOperationCode.UserDisconnected:
                            this.DisconnectedUserEvent?.Invoke(message);
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
