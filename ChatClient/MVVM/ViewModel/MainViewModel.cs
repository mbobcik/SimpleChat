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
        public RelayCommand ConnectToServerCommand{ get; set; }
        public string UserName { get; set; }
        public ObservableCollection<UserModel> ConnectedUsers{ get; set; }

        private ServerConnector server;
        
        public MainViewModel() {
            server = new ServerConnector();
            server.ConnectedEvent += UserConnected;
            ConnectToServerCommand = new RelayCommand (x =>  server.Connect(UserName), x => !string.IsNullOrEmpty(UserName));
            ConnectedUsers = new ObservableCollection<UserModel>();
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
