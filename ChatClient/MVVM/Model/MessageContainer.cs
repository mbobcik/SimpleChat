using System;
using System.Net.WebSockets;

namespace ChatClient.MVVM.Model
{
    internal class MessageContainer
    {
        public UserModel Sender { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime SentAt { get; internal set; }
        public string Message { get; set; }

        public MessageType Type { get; set; }

        public string ShowMessage 
        { 
            get 
            {
                switch (Type)
                {
                    case MessageType.Message:
                        return $"[{SentAt}] {Sender}: {Message}"; 
                    case MessageType.DisconnectedUser:
                        return $"[{SentAt}] User {Sender} Disconnected!";
                    default:
                        return base.ToString();
                }
            } 
        }

        public enum MessageType
        {
            Message = 0,
            DisconnectedUser
        }
    }
}