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
            Console.WriteLine("enter your name");
            string data = Console.ReadLine();
            Chat chat = new Chat(data);
            chat.SendMessage();
            
            
            Thread ListenThread = new Thread(new ThreadStart(chat.Listen));
            ListenThread.Start();
            Thread ListenThread2 = new Thread(new ThreadStart(chat.TCPListen));
            ListenThread2.Start();
           
              
            

            while (true)
            {
                 data = Console.ReadLine();
                chat.BroadcastMessage(data);
            }



        }
    }
}
