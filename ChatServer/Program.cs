using Chat.Shared;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ChatServer
{
    internal class Program
    {
        static TcpListener? listener;
        static List<ClientContainer> connectedClients = new List<ClientContainer>();  
        static PaketBuilder paketBuilder = new PaketBuilder();
        static ConfigurationProvider<ServerConfiguration> configurationProvider;
        static void Main(string[] args)
        {
            configurationProvider = new ConfigurationProvider<ServerConfiguration>("Configuration.json");
            
            var port = configurationProvider.Configuration.Port;
            Console.WriteLine($"Hello, World! Listening at {port}");
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (true)
            { 
                var client = new ClientContainer(listener.AcceptTcpClient());
                connectedClients.Add(client);
                // End at https://youtu.be/I-Xmp-mulz4?t=892
                BroadcastNewConnection();
            }
        }

        static void BroadcastNewConnection()
        {
            foreach (var clientDestination in connectedClients)
            {
                foreach (var clientMessage in connectedClients)
                {
                    var broadcastPaket = paketBuilder.BuildMessage(NetworkOperationCode.NewClientBroadcast, clientMessage.UserName, clientMessage.Id.ToString());
                    clientDestination.Socket.Client.Send(broadcastPaket);
                }
            }
        }
        
        internal static void HandleDisconnectedUser(Guid Id)
        {
            connectedClients.RemoveAll(client => client.Id == Id);
            Broadcast(NetworkOperationCode.UserDisconnected, Id.ToString(), DateTime.Now.ToString());
        }

        internal static void Broadcast(NetworkOperationCode opCode, params string[] message)
        {
            foreach(var clientDestination in connectedClients)
            {
                var messagePacket = paketBuilder.BuildMessage(opCode, message);
                clientDestination.Socket.Client.Send(messagePacket);
            }
        }
    }
}