
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DomainModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EchoServer;
using System.Threading;

namespace XUnitTest
{
    public static class Helper
    {

        private static Server _myServer { get; set; }


        internal static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }


        internal static T FromJson<T>(this string element)
        {
            return JsonConvert.DeserializeObject<T>(element);
        }


        #region    Region  - Send and Request ...commented
        /*
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

            response = new Response { Body = "", Status = statTxt };
            var jsonObj = ToJson(response);
            byte[] msg = Encoding.UTF8.GetBytes(jsonObj);
            client.GetStream().Write(msg, 0, msg.Length);
        }
        */


        /*
    internal static Response ReadResponse(this TcpClient client)
    {
        try
        {
            var e = client.SendBufferSize;
            NetworkStream strm = client.GetStream();


            var x = strm.DataAvailable;     //todo - currently false

            byte[] resp = new byte[client.ReceiveBufferSize];
            int bytesread = strm.Read(resp, 0, resp.Length);
            string responseData = Encoding.UTF8.GetString(resp, 0, bytesread);
            return FromJson<Response>(responseData);
        }
        catch (Exception ex)
        {
            var x = ex.Message;
        }
        return null;
    }
    */
        #endregion


        internal static void SendRequest(this TcpClient client, string request)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(buffer, 0, buffer.Length);          
        }



        internal static Response ReadResponse(this TcpClient client)
        {
            try
            {                
                NetworkStream strm = client.GetStream();
                while (!strm.DataAvailable)
                {
                    Thread.Sleep(500);
                }
             
                byte[] resp = new byte[client.ReceiveBufferSize];
                int bytesread = strm.Read(resp, 0, resp.Length);
                string responseData = Encoding.UTF8.GetString(resp, 0, bytesread);
                strm.Close();
                client.Dispose();
                _myServer.KillServer();               
                return FromJson<Response>(responseData);
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
            return null;
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

            _myServer = new Server();
            _myServer.StartServer();

            var client = new TcpClient();
            client.Connect(_myServer._Address, _myServer._Port);
            return client;
        }


    }
}
