﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
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

               string htmlBody =
            "<html><body><h1>Error</h1></body></html>";
                NetworkStream stream = client.GetStream();
                string[] temp = Encoding.ASCII.GetString(buf).Trim().Split(new char[] { '\r', '\n' });
                string req = temp.FirstOrDefault(x => x.Contains("Host"));
                req = req.Substring(req.IndexOf(" ") + 1);
                
                var blacklist = ConfigurationManager.AppSettings;
                foreach (var key in blacklist.AllKeys)
                {
                    if (parser(req).Equals(key))
                    {
                        stream.Write(Encoding.ASCII.GetBytes(htmlBody),0, Encoding.ASCII.GetBytes(htmlBody).Length);
                        return;
                    }
                }
                var server = new TcpClient(req, 80);
                NetworkStream servStream = server.GetStream();
                servStream.Write(buf, 0, buf.Length);
                var respBuf = new byte[32];

                //this is to capture status of http request and log it.

                servStream.Read(respBuf, 0, respBuf.Length);

                stream.Write(respBuf, 0, respBuf.Length);

                var headers = Encoding.UTF8.GetString(respBuf).Split(new char[] { '\r', '\n' });

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

        public static string parser(string name)
        {
            if (name.Contains("www."))
            {
               return name.Replace("www.", string.Empty);
            }
            if (name.Contains("https://www."))
            {
                return name.Replace("https://www.", string.Empty);
            }
            if (name.Contains("https://"))
            {
                return name.Replace("https://", string.Empty);
            }
            return name;
        }

    }
   
}
