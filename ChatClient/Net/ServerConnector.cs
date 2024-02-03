using Chat.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    internal class ServerConnector
    {
        public event Action<PaketContainer>? ConnectedEvent;
        public event Action<PaketContainer>? MessageReceivedEvent;
        public event Action<PaketContainer>? DisconnectedUserEvent;

        private PaketBuilder packetBuilder;
        private TcpClient client;

        private string username;

        public ServerConnector()
        {
            client = new TcpClient();
            packetBuilder = new PaketBuilder();
        }

        public async Task ConnectAsync(string username, string addressWithPort, CancellationToken cancelationToken)
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
                    this.username = username;
                }

                ReadPackets(paketReader, cancelationToken);
            }
        }
        public async Task DisconnectAsync()
        {
            if (this.client.Connected)
            {
                var disconnectPacket = this.packetBuilder.BuildMessage(NetworkOperationCode.UserDisconnected);
                this.client.Client.Send(disconnectPacket);
                this.client.Close();
            }
        }

        public void SendMessageToServer(string message)
        {
            var sentAt = DateTime.Now.ToString();
            var messagePacket = this.packetBuilder.BuildMessage(NetworkOperationCode.MessageToServer, message, sentAt);
            this.client.Client.Send(messagePacket);
        }

        private void ReadPackets(PaketReader paketReader, CancellationToken cancelationToken)
        {
            Task.Factory.StartNew(() =>
            {
                string log;
                PaketContainer message;
                while (!cancelationToken.IsCancellationRequested)
                {
                    try
                    {
                        message = paketReader.ReadMessage();
                    }
                    catch (IOException e)
                    {
                        if (cancelationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        throw;
                    }

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
            }, cancelationToken);
        }
    }
}
