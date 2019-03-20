using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace P2PChat
{
   public class Chat
    {
     //   const int TCPMessagePort = 3200;
        private UdpClient udpclient;
        private IPAddress multicastaddress;
        private IPEndPoint remoteep;
      //  public void SendOpenMessage(string data);
      //  public void Listen();
        public Chat()
        {
            multicastaddress = IPAddress.Parse("239.0.0.222"); // один из зарезервированных для локальных нужд UDP адресов
            udpclient = new UdpClient();
            ConnectedUser = new List<UDPUser>();
            udpclient.JoinMulticastGroup(multicastaddress);
            remoteep = new IPEndPoint(multicastaddress, 2222);
        }
        public void SendMessage(string data)
        {
            Byte[] buffer = Encoding.UTF8.GetBytes(data);

            udpclient.Send(buffer, buffer.Length, remoteep);
        }
        private int Number = 0;
        private List<UDPUser> ConnectedUser { get; set; }
        

        public void Listen()
        {
            UdpClient client = new UdpClient();

            client.ExclusiveAddressUse = false; //one port for many users
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 2222);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); //Позволяет ограничить сокет, разрешив только тот адрес, который уже используется.
            client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);

            client.JoinMulticastGroup(multicastaddress);

            Console.WriteLine("\tListening started");

            string formatted_data;

            while (true)
            {
                Byte[] data = client.Receive(ref localEp);
                
                formatted_data = Encoding.UTF8.GetString(data);
              //  Console.WriteLine(formatted_data);
                var username = "User" + formatted_data;
                ConnectedUser.Add(new UDPUser()
                {
                //    chatConnection = null,
                    username = username,
                    ipAddress = localEp.Address,
                    IsConnected = true

                });
                Console.WriteLine(ConnectedUser[Number].username);
                Console.WriteLine(ConnectedUser[Number].ipAddress);
                Number++;
                //Task.Factory.StartNew(() => TcpConnection(number));
                
            }
        }
        //public NetworkStream stream;
        //public void  TcpConnection(int userEntryIndex)
        //{
        //    Check if user's tcpConnection is open, if not, open it

        //    if (connectedUser[userEntryIndex].chatConnection != null
        //    && connectedUser[userEntryIndex].chatConnection.Connected)
        //        return;

        //    var newConnection = new TcpClient();
            
        //    newConnection.Connect(new IPEndPoint(connectedUser[userEntryIndex].ipAddress, TCPMessagePort));
        //    stream = newConnection.GetStream();
        //    connectedUser[userEntryIndex].chatConnection = newConnection;

          
        //}
        //public void SendMessageTCP(string message)
        //{
        //    connectedUser.ForEach(async client =>
        //    {
        //        var clientStream = client.chatConnection.GetStream();
        //        var messageBytes = Encoding.ASCII.GetBytes(message + "\r\n");
        //        await clientStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        //    });

        //}
        
        //protected internal void ListenTCP()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            byte[] data = new byte[64]; // буфер для получаемых данных
        //            StringBuilder builder = new StringBuilder();
        //            int bytes = 0;
        //            do
        //            {
        //                bytes = stream.Read(data, 0, data.Length);
        //                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        //            }
        //            while (stream.DataAvailable);

        //            string message = builder.ToString();
        //            Console.WriteLine(message);//вывод сообщения
        //        }
        //        catch
        //        {
        //            Console.WriteLine("Подключение прервано!"); //соединение было прервано
        //            Console.ReadLine();
                   
        //        }
        //    }
        //}

    }
}
