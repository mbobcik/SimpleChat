using Chat.Shared;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand DisconnectFromServerCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string UserName { get; set; } = string.Empty;
        public string AddressWithPort { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        public ObservableCollection<UserModel> ConnectedUsers{ get; set; }
        public ObservableCollection<MessageContainer> Messages { get; set; }

        private ServerConnector server;
        private CancellationTokenSource cancellationTokenSource;

        public MainViewModel() {
            this.cancellationTokenSource = new CancellationTokenSource();
            server = new ServerConnector();

            server.ConnectedEvent += UserConnected;
            server.DisconnectedUserEvent += DisconnectedUser;
            server.MessageReceivedEvent += ReceivedMessage;
            server.LogErrorEvent += LogErrorEvent;

            ConnectToServerCommand = new RelayCommand(async x => await server.ConnectAsync(UserName, AddressWithPort, cancellationTokenSource.Token), 
                x => !string.IsNullOrWhiteSpace(UserName) && AddressHelper.ValidateServerAddress(AddressWithPort));
            SendMessageCommand = new RelayCommand(x => SendMessage(), x => !string.IsNullOrWhiteSpace(Message));
            DisconnectFromServerCommand = new RelayCommand(x => Disconnect());

            ConnectedUsers = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<MessageContainer>();
        }

        private void LogErrorEvent(string message)
        {
            var errorMessageContainer = new MessageContainer
            {
                Message = message,
                ReceivedAt = DateTime.Now,
                Type = MessageContainer.MessageType.ErrorMessage
            };
            Application.Current.Dispatcher.Invoke(() => Messages.Add(errorMessageContainer));
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or whitespace.", nameof(propertyName));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SendMessage()
        {
            server.SendMessageToServer(this.Message);
            this.Message = string.Empty;
            // Notify UI that property was changed
            OnPropertyChanged(nameof(Message));
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
