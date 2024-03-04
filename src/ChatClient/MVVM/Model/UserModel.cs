using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.Model
{
    internal class UserModel
    {
        public string UserName { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public override string ToString()
        {
            return $"[{UserName}]";
        }
    } 
}
