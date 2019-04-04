using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PChat
{

   public class ChatHistrory
    {
        const int TCPHistoryPort = 13000;
        private TcpListener tcpListener { get; set; }
        public ChatHistrory()
        {

        }
        public  void HistoryListen(){
            tcpListener = new TcpListener(IPAddress.Any, TCPHistoryPort);
            tcpListener.Start();
            while (true)
            {
                var newClient = tcpListener.AcceptTcpClient();
                
                var localStream = new MemoryStream();
                var writer = new StreamWriter(localStream);
                foreach (var line in Chat.HistoryList)
                {
                    writer.WriteLine(line);
                }

                writer.Flush();
                localStream.Seek(0, SeekOrigin.Begin);
                localStream.CopyTo(newClient.GetStream());
                newClient.Close();
            }
        }
    }
}
