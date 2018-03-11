
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using DomainModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EchoServer;
using System.Diagnostics;

namespace XUnitTest
{
    public static class Helper
    {

        private static Server _myServer { get; set; }
        private static int _QuestionCounter { get; set; } = 0;


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
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(request);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                LogAnError(ex);
            }           
        }



        internal static Response ReadResponse(this TcpClient client, int testCaseCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string responseData = "";
            try
            {
                using (NetworkStream strm = client.GetStream())
                {
                    while (!strm.DataAvailable)
                    {
                        if (stopwatch.Elapsed.TotalSeconds > 20)
                        {
                            LogAnError(new InvalidOperationException("Client could not receive response, so stopwatch elapsed time reached max allowed"));
                            return null;
                        }
                    }
                    byte[] resp = new byte[client.ReceiveBufferSize];
                    int bytesread = strm.Read(resp, 0, resp.Length);
                    responseData = Encoding.UTF8.GetString(resp, 0, bytesread);
                }
               
                client.Dispose();
                _QuestionCounter++;
                if (_QuestionCounter == testCaseCount)               
                _myServer.KillServer();
                return FromJson<Response>(responseData);
            }
            catch (Exception ex)
            {
                LogAnError(ex);
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
            if (_myServer == null)
            {
                _QuestionCounter = 0;
                _myServer = new Server();
                _myServer.StartServer();
            }

            var client = new TcpClient();
            client.Connect(_myServer._Address, _myServer._Port);
            return client;
        }


        internal static void LogAnError(Exception ex)
        {
            FileStream fs = new FileStream("TestLogger.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("Error at " + DateTime.Now.ToString() + "    -  due to " + ex.Message);
            sw.Close();
            fs.Close();
        }



    }
}
