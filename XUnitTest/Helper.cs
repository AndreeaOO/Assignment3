
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DomainModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace XUnitTest
{
    public static class Helper
    {
        internal const int Port = 5000;

        internal static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        internal static T FromJson<T>(this string element)
        {
            return JsonConvert.DeserializeObject<T>(element);
        }

        internal static void SendRequest(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }

        internal static Response ReadResponse(this TcpClient client)
        {
            var strm = client.GetStream();
            //strm.ReadTimeout = 250;
            byte[] resp = new byte[2048];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);

                } while (bytesread == 2048);

                var responseData = Encoding.UTF8.GetString(memStream.ToArray());
                return JsonConvert.DeserializeObject<Response>(responseData);
            }
        }


        internal static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }

        internal static TcpClient Connect()
        {
            IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
            var client = new TcpClient();
            client.Connect(ipLocalEndPoint);
            return client;
        }



    }
}
