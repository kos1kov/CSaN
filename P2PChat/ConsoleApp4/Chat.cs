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
        const int TCPMessagePort = 8888;
        private UdpClient udpclient;
    //    private TcpClient tcpclient;
    //    private NetworkStream stream;
        private string ClientName;
        private IPAddress multicastaddress;
        private IPEndPoint remoteep;
        private TcpListener tcpListener { get; set; }
        //  public void SendOpenMessage(string data);
        //  public void Listen();
        public Chat(string name)
        {
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
                if (ConnectedUser.Find(x => x.ipAddress.ToString() == localEp.Address.ToString()) == null)
                {
                    formatted_data = Encoding.UTF8.GetString(data);
                    if (formatted_data != ClientName)
                    {


                        var username = "User" + formatted_data;
                        ConnectedUser.Add(new UDPUser()
                        {
                            chatConnection = null,
                            username = username,
                            ipAddress = localEp.Address,
                            IsConnected = true

                        });
                        Console.WriteLine(ConnectedUser[Number].username);
                        Console.WriteLine(ConnectedUser[Number].ipAddress);
                        initTCP(Number);
                        Number++;

                    }
                }
            }
        }
        private void initTCP(int index)
        {
            if (ConnectedUser[index].chatConnection != null && ConnectedUser[index].chatConnection.Connected)
                return;
            SendMessage();
            var newtcpConnect = new TcpClient();
            newtcpConnect.Connect(new IPEndPoint(ConnectedUser[index].ipAddress, TCPMessagePort)); //establish connection
            ConnectedUser[index].chatConnection = newtcpConnect;
           
        }
        public void TCPListen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    var address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
                    ConnectedUser.Add(new UDPUser()
                    {
                        chatConnection = tcpClient,
                        ipAddress = address,
                        IsConnected = true
                    });
                   
                    
                    Thread tcp = new Thread(() => TcpMessage(tcpClient, "u", true));
                    tcp.Start();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
               
            }
        }
        private void TcpMessage(TcpClient connection, string username, bool IsLocalConnection)
        {
            NetworkStream stream = connection.GetStream();
            while (IsLocalConnection) {
               
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
            }
            
        }
        protected internal void BroadcastMessage(string message)
        {
            ConnectedUser.ForEach(async client =>
            {
                var clientStream = client.chatConnection.GetStream();
                var messageBytes = Encoding.UTF8.GetBytes(message + "\r\n");
                
                await clientStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            });
        }

    }
}
