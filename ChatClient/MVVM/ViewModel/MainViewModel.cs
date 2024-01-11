using Chat.Shared;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    internal class MainViewModel
    {
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        
        public string UserName { get; set; }
        public string Message { get; set; }

        public ObservableCollection<UserModel> ConnectedUsers{ get; set; }
        public ObservableCollection<MessageContainer> Messages { get; set; }

        private ServerConnector server;
        
        public MainViewModel() {
            server = new ServerConnector();

            server.ConnectedEvent += UserConnected;
            server.DisconnectedUserEvent += DisconnectedUser;
            server.MessageReceivedEvent += ReceivedMessage; 

            ConnectToServerCommand = new RelayCommand (x =>  server.Connect(UserName), x => !string.IsNullOrEmpty(UserName));
            SendMessageCommand = new RelayCommand(x => server.SendMessageToServer(Message), x => !string.IsNullOrEmpty(Message));

            ConnectedUsers = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<MessageContainer>();
        }

        private void DisconnectedUser(PaketContainer message)
        {
            var disconnectedUserMessageContainer = new MessageContainer
            {
                Sender = ConnectedUsers.First(x => x.Id.ToString() == message.Payload[0]),
                SentAt = DateTime.Parse(message.Payload[1]),
                ReceivedAt = DateTime.Now,
                Type = MessageContainer.MessageType.DisconnectedUser
            };
            Application.Current.Dispatcher.Invoke(() => { 
                Messages.Add(disconnectedUserMessageContainer);
                ConnectedUsers.Remove(disconnectedUserMessageContainer.Sender);
            });
        }

        private void ReceivedMessage(PaketContainer message)
        {
            var receivedMessageContainer = new MessageContainer
            {
                Sender = ConnectedUsers.First(x => x.Id.ToString() == message.Payload[0]),
                Message = message.Payload[1],
                ReceivedAt = DateTime.Now,
                SentAt = DateTime.Parse(message.Payload[2]),
                Type = MessageContainer.MessageType.Message
            };
            Application.Current.Dispatcher.Invoke(() => Messages.Add(receivedMessageContainer));
        }

        private void UserConnected(PaketContainer paket)
        {
            var user = new UserModel();
            user.UserName = paket.Payload.ElementAt(0);
            user.Id = Guid.Parse(paket.Payload.ElementAt(1));

            if (!ConnectedUsers.Any(x => x.Id == user.Id))
            {
                Application.Current.Dispatcher.Invoke(() => ConnectedUsers.Add(user));
            }
        }
    }
}
