using DomainModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var addr = IPAddress.Parse("127.0.0.1");
            var server = new TcpListener(addr, 5000);

            server.Start();
            Console.WriteLine("Server started ...");


            while (true)
            {
                var client = server.AcceptTcpClient();

                var strm = client.GetStream();

                var buffer = new byte[client.ReceiveBufferSize];
                var readCnt = strm.Read(buffer, 0, buffer.Length);

                var payload = Encoding.UTF8.GetString(buffer, 0, readCnt);
                var request = JsonConvert.DeserializeObject<Request>(payload);

                Console.WriteLine(request.Body);

                var res = Encoding.UTF8.GetBytes(request.Body.ToUpper());

                strm.Write(res, 0, res.Length);

            }

            //server.Stop();

        }
    }
}