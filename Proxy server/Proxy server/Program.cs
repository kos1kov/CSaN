using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy_server
{
    class Program
    {
       
        static void Main(string[] args)
        {
            //var listener = new HttpListener(8080);
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1") ,8080);
            listener.Start();

            while (true)
            {
                var client = listener.AcceptTcpClient();

                Thread thread = new Thread(() => RecvData(client));
                thread.Start();
            }


        }
       public static void RecvData(TcpClient client)
        {
            
            NetworkStream stream = client.GetStream();
            byte[] buf;
            buf = new byte[16000];
            while (true)
            {
                if (!stream.CanRead)
                    return;
                if (stream.Read(buf,0,buf.Length).Equals(0))
                    return;
               // stream.Read(buf, 0, buf.Length);
                HTTPserv(buf, client);
            }
         
       }


        public static void HTTPserv(byte[] buf,TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                string[] temp = Encoding.ASCII.GetString(buf).Trim().Split(new char[] { '\r', '\n' });
                string req = temp.FirstOrDefault(x => x.Contains("Host"));
                req = req.Substring(req.IndexOf(" ") + 1);
                

                var server = new TcpClient(req, 80);
                NetworkStream servStream = server.GetStream();
                servStream.Write(buf, 0, buf.Length);
                var responseBuffer = new byte[32];

                //this is to capture status of http request and log it.

                servStream.Read(responseBuffer, 0, responseBuffer.Length);

                stream.Write(responseBuffer, 0, responseBuffer.Length);

                var headers = Encoding.UTF8.GetString(responseBuffer).Split(new char[] { '\r', '\n' });

                string ResponseCode = headers[0].Substring(headers[0].IndexOf(" ") + 1);
                Console.WriteLine($"\n{req} {ResponseCode}");
                servStream.CopyTo(stream);
                
            }
            catch
            {
                return;
            }
            finally
            {
                client.Dispose();
            }
           
        }



    }
   
}
