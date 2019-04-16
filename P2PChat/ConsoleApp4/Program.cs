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
            try
            {

                Console.WriteLine("enter your name");
                string data = Console.ReadLine();
                Chat chat = new Chat(data);
                ChatHistrory chatHistrory = new ChatHistrory();
                chat.SendMessage();


                Thread ListenThread = new Thread(new ThreadStart(chat.Listen));
                ListenThread.Start();
                
                Console.WriteLine();
               
                Thread ListenThread2 = new Thread(new ThreadStart(chat.TCPListen));
                ListenThread2.Start();

                Thread ListenThread3 = new Thread(new ThreadStart(chatHistrory.HistoryListen));
                ListenThread3.Start();

                if (Chat.HistoryList.Count != 0)
                {
                    Console.WriteLine("history start");
                    foreach (var text in Chat.HistoryList)
                    {
                        Console.WriteLine(text);
                    }
                    Console.WriteLine("history end");
                }



                while (true)
                {
                    data = Console.ReadLine();
                    Console.WriteLine(DateTime.Now.ToLongTimeString());
                    Console.WriteLine();
                    
                    if(data == "!history")
                    {
                        chat.RecvHistory();
                        Thread.Sleep(2000);
                        if (Chat.HistoryList.Count != 0)
                        {
                            Console.WriteLine("history start");
                            foreach (var text in Chat.HistoryList)
                            {
                                Console.WriteLine(text);
                            }
                            Console.WriteLine("history end");
                        }
                    }
                    else
                    {
                        chat.BroadcastMessage(data);
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
