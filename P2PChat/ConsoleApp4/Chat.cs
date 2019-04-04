using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace P2PChat
{
   public class Chat
    {
        const int TCPMessagePort = 8888;
        const int TCPHistoryPort = 13000;
        private UdpClient udpclient;
        private string ClientName;
        public static List<string> HistoryList;
        private IPAddress multicastaddress;
        private IPEndPoint remoteep;
        private TcpListener tcpListener { get; set; }
        public Chat(string name)
        {
            HistoryList = new List<string>();
            ClientName = name;
            multicastaddress = IPAddress.Parse("239.0.0.222"); // один из зарезервированных для локальных нужд UDP адресов
            udpclient = new UdpClient();
            ConnectedUser = new List<UDPUser>();
            udpclient.JoinMulticastGroup(multicastaddress);
            remoteep = new IPEndPoint(multicastaddress, 2222);
           
        }
        public void SendMessage()
        {
            Byte[] buffer = Encoding.UTF8.GetBytes(ClientName);

            udpclient.Send(buffer, buffer.Length, remoteep);

        }
       
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
                int Number = 0;
                Byte[] data = client.Receive(ref localEp);
                if (ConnectedUser.Find(x => x.ipAddress.ToString() == localEp.Address.ToString()) == null)
                {
                    formatted_data = Encoding.UTF8.GetString(data);
                    if (formatted_data != ClientName)
                    {

                        ConnectedUser.Add(new UDPUser()
                        {
                            chatConnection = null,
                            username = formatted_data,
                            ipAddress = localEp.Address,
                            IsConnected = true

                        });
                        
                        Console.WriteLine("USER " + ConnectedUser[Number].username +" Connected");
                        HistoryList.Add("USER " + ConnectedUser[Number].username + " Connected" + " " );
                        Number = ConnectedUser.FindIndex(x => x.ipAddress.ToString() == localEp.Address.ToString());
                        initTCP(Number);
                        

                    }
                }
            }
        }
        private void initTCP(int index)
        {

            SendMessage();
            var newtcpConnect = new TcpClient();
            newtcpConnect.Connect(new IPEndPoint(ConnectedUser[index].ipAddress, TCPMessagePort)); //establish connection
            ConnectedUser[index].chatConnection = newtcpConnect;
           
        }
        public void TCPListen()
        {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                IPAddress address1 = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
                string Name = ConnectedUser.Find(x => x.ipAddress.ToString() == address1.ToString()).username;
                    Thread tcp = new Thread(() => TcpMessage(tcpClient, Name, true));
                    tcp.Start();
                    
                }
            
        }
        private void TcpMessage(TcpClient connection, string username, bool IsLocalConnection)
        {
            NetworkStream stream = connection.GetStream();
            try
            {
                while (IsLocalConnection)
                {

                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    string message;
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = builder.ToString();
                    
                    Console.WriteLine(message);
                    Console.WriteLine(DateTime.Now.ToLongTimeString() +"\n");
                    HistoryList.Add(message  +" " + DateTime.Now.ToLongTimeString() + "\n");
                }
            }
            catch
            {
                Console.WriteLine(username +" покинул чат"); //соединение было прервано
                HistoryList.Add(username + " покинул чат");
                var address = ((IPEndPoint)connection.Client.RemoteEndPoint).Address;
                ConnectedUser.RemoveAll(X => X.ipAddress.ToString() == address.ToString());
                Console.WriteLine(address);
                if (stream != null)
                    stream.Close();//отключение потока
                if (connection != null)
                    connection.Close();//отключение клиента
                
            }
            
        }
        protected internal void BroadcastMessage(string message)
        {
           
            message = ClientName + ": " + message;
            HistoryList.Add(message + " " + DateTime.Now.ToLongTimeString());
            var messageBytes = Encoding.UTF8.GetBytes(message);
            ConnectedUser.ForEach( client =>
            {
                var clientStream = client.chatConnection.GetStream();

                 clientStream.Write(messageBytes, 0, messageBytes.Length);
            });
        }
        public void RecvHistory()
        {
            if(ConnectedUser.Count == 0)
            {
                return;
            }
            TcpClient HistoryClient = new TcpClient();
            try
            {
                HistoryClient.Connect(new IPEndPoint(ConnectedUser[0].ipAddress, TCPHistoryPort));

                var connectionStream = HistoryClient.GetStream();
                var History = new StreamReader(connectionStream);
                while (true)
                {
                    
                    string line;
                    if ((line = History.ReadLine()) != null)
                    {
                       HistoryList.Add(line);
                    }
                    else
                        return;
                }

            }
            catch { return; }

        }
    }
}
