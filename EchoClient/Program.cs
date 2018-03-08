using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DomainModel;
using Newtonsoft.Json;

namespace EchoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"),5000);

            var strm = client.GetStream();



            var request = new Request
            {
                Body = "Hello"
            };

            var payload = JsonConvert.SerializeObject(request);
            var buffer = Encoding.UTF8.GetBytes(payload);

            strm.Write(buffer, 0, buffer.Length);

            strm.Close();

            client.Dispose();
        }
    }
}
