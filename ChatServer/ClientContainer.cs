using Chat.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class ClientContainer
    {
        public string UserName { get; private set; }
        public Guid Id { get; private set; }
        public TcpClient Socket{ get; private set; }

        PaketReader paketReader;

        public ClientContainer(TcpClient socket)
        {
            Socket = socket;
            Id = Guid.NewGuid();
            paketReader = new PaketReader(Socket.GetStream());

            var message = paketReader.ReadMessage();
            if (message.OperationCode != NetworkOperationCode.NewConnection)
            {
                throw new Exception($"First {typeof(NetworkOperationCode)} is not {nameof(NetworkOperationCode.NewConnection)}. // TODO: Find Better ExceptionType");
            }

            UserName = message.Payload.First();

            // End on https://youtu.be/I-Xmp-mulz4?t=1566

            Console.WriteLine($"[{DateTime.Now}]: User {UserName} with ID {Id} has connected");

            Task.Factory.StartNew(Process);
        }

        void Process()
        {
            while ( true )
            {
                try
                {
                    var message = this.paketReader.ReadMessage();
                    switch ( message.OperationCode )
                    {
                        case NetworkOperationCode.MessageToServer:
                            string content = message.Payload[0];
                            string sentAt = message.Payload[1];
                            DateTime sentAtDateTime = DateTime.Parse( sentAt );
                            Console.WriteLine($"[{DateTime.Now}]: user [{UserName}:{Id}] sent message at {sentAtDateTime}: {content}");
                            Program.Broadcast(NetworkOperationCode.MessageToServer, Id.ToString(), content, sentAt);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e) // TODO BetterException and error handling
                {
                    Console.WriteLine($"[{DateTime.Now}] user [{UserName}:{Id}] disconnected. Exception {e}");
                    Socket.Close();
                    Program.HandleDisconnectedUser(Id);
                    break;
                }

            }
        }
    }
}
