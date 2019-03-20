using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PChat
{
   public class UDPUser
    {
        public TcpClient chatConnection;
        public string username;
        public IPAddress ipAddress; 
        public bool IsConnected;
    }
}
