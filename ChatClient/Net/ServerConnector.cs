using Chat.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Printing;
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
        public event Action<string>? LogErrorEvent;

        private PaketBuilder packetBuilder;
        private TcpClient client;

        private string username = String.Empty;
        private bool connecting = false;

        public ServerConnector()
        {
            client = new TcpClient();
            packetBuilder = new PaketBuilder();
        }

        public async Task ConnectAsync(string? username, string? addressWithPort, CancellationToken cancelationToken)
        {
            if (!client.Connected && !this.connecting)
            {
                try
                {
                    this.connecting = true;
                    await client.ConnectAsync(AddressHelper.ParseAddress(addressWithPort), AddressHelper.ParsePort(addressWithPort));
                }
                catch (Exception ex)
                {
                    this.LogError($"ERROR: Failed to connect to client with following exception: {ex}");
                }
                finally
                {
                    this.connecting = false;
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
                await this.client.Client.SendAsync(disconnectPacket);
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
                    catch (IOException)
                    {
                        if (cancelationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        throw;
                    }

                    switch (message.OperationCode)
                    {
                        case NetworkOperationCode.NewClientBroadcast:
                            this.ConnectedEvent?.Invoke(message);
                            break;
                        case NetworkOperationCode.MessageToServer:
                            this.MessageReceivedEvent?.Invoke(message);
                            break;
                        case NetworkOperationCode.UserDisconnected:
                            this.DisconnectedUserEvent?.Invoke(message);
                            break;
                        case NetworkOperationCode.None:
                        case NetworkOperationCode.NewConnection:
                        default:
                            log = string.Join("}, {", message.Payload.ToArray());
                            this.LogError($"This should not happen. OpCode {message.OperationCode}; Payload {{{log}}}");
                            break;
                    }
                }
            }, cancelationToken);
        }

        private void LogError(string format, params string[] arguments)
        {
            this.LogErrorEvent?.Invoke(string.Format(format, arguments));
        }
    }
}
