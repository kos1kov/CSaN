using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace P2PChat
{
    class Program
    {
        static void Main(string[] args)
        {

            Chat chat = new Chat();
            
            Thread ListenThread = new Thread(new ThreadStart(chat.Listen));
            ListenThread.Start();

                string data = Console.ReadLine();
                chat.SendMessage(data);
          
          
            // data = Console.ReadLine();
            // chat.SendMessageTCP(data);
            // Thread ListenThread2 = new Thread(new ThreadStart(chat.ListenTCP));
            // ListenThread2.Start();


        }
    }
}
