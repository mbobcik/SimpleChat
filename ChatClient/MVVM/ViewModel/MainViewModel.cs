using Chat.Shared;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    internal class MainViewModel
    {
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand DisconnectFromServerCommand { get; set; }

        public string UserName { get; set; }
        public string AddressWithPort{ get; set; }
        public string Message { get; set; }
        
        public ObservableCollection<UserModel> ConnectedUsers{ get; set; }
        public ObservableCollection<MessageContainer> Messages { get; set; }

        private ServerConnector server;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancelationToken;

        public MainViewModel() {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancelationToken = cancellationTokenSource.Token;
            server = new ServerConnector();

            server.ConnectedEvent += UserConnected;
            server.DisconnectedUserEvent += DisconnectedUser;
            server.MessageReceivedEvent += ReceivedMessage;

            ConnectToServerCommand = new RelayCommand(async x => await server.ConnectAsync(UserName, AddressWithPort, this.cancelationToken), x => !string.IsNullOrWhiteSpace(UserName) && AddressHelper.ValidateServerAddress(AddressWithPort));
            SendMessageCommand = new RelayCommand(x => SendMessage(), x => !string.IsNullOrWhiteSpace(Message));
            DisconnectFromServerCommand = new RelayCommand(x => Disconnect());

            ConnectedUsers = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<MessageContainer>();
        }

        private void SendMessage()
        {
            server.SendMessageToServer(this.Message);
        }

        private async void Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            await this.server.DisconnectAsync();
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
