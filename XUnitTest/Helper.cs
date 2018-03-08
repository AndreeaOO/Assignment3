
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
        internal const int _Port = 5000;

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
            Request requestObj = JsonConvert.DeserializeObject<Request>(request);

            var response = new Response();
            string statTxt = StatusResponse.GetStatusCodeText(StatusResponse.STATUSCODE.BADREQUEST);
            if (requestObj == null)
                  StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.DEFAULT, ref statTxt);          
            else if (string.IsNullOrEmpty(requestObj.Method))
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.METHOD, ref statTxt);
            else if (string.IsNullOrEmpty(requestObj.Path) && requestObj.Method != "echo")
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.PATH, ref statTxt);
            else if (requestObj.Date <= 0)
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.DATE, ref statTxt);

            response = new Response { Body = null, Status = statTxt };
            var jsonObj = JsonConvert.SerializeObject(response);
            var msg = Encoding.UTF8.GetBytes(jsonObj);
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
                    try
                    {
                        bytesread = strm.Read(resp, 0, client.ReceiveBufferSize);
                        memStream.Write(resp, 0, bytesread);
                    }
                    catch (Exception ex)
                    {
                        var x = ex.Message;
                    }                 

                } while (bytesread == 2048);

                string responseData = Encoding.UTF8.GetString(memStream.ToArray());
                return JsonConvert.DeserializeObject<Response>(responseData);
            }
        }


        internal static string UnixTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }


        internal static TcpClient Connect()
        {
            /* IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
            var client = new TcpClient();
            client.Connect(ipLocalEndPoint);
            return client; */

            var addr = IPAddress.Parse("127.0.0.1");
            var server = new TcpListener(addr, _Port);
            server.Start();

            var client = new TcpClient();
            client.Connect(addr, _Port);        
            return client;

        }



    }
}
