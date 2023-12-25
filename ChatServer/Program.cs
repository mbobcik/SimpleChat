using Chat.Shared;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    internal class Program
    {
        static TcpListener listener;
        static List<ClientContainer> connectedClients;
        static PaketBuilder paketBuilder;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Listening at 7981");
            connectedClients = new List<ClientContainer>();
            paketBuilder = new PaketBuilder();

            listener = new TcpListener(IPAddress.Any, 7981);
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
    }
}