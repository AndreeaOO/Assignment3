using DomainModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class Server
    {

        public int _Port { get; } = 5000;
        public TcpListener _Listener { get; private set; }
        public IPAddress _Address { get; } = IPAddress.Parse("127.0.0.1");
        public bool _KeepListening { get; private set; } = true;


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



        /// <summary> Start the server instance </summary>
        public void StartServer()
        {
            _Listener = new TcpListener(_Address, _Port);
            _Listener.Start();
            _Listener.Server.Listen(10);
            StartListeningAsync();
        }


        /// <summary>  </summary>
        public void KillServer()
        {
            _KeepListening = false;
        }


        /// <summary>  </summary>
        private async Task<bool> StartListeningAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (_KeepListening)
            {
                try
                {
                    using (TcpClient c = await _Listener.AcceptTcpClientAsync())
                    {
                        using (NetworkStream ns = c.GetStream())
                        {
                            while (!ns.DataAvailable)
                            {
                                if (stopwatch.Elapsed.TotalSeconds > 20)
                                    break;
                            }
                            if (ns.DataAvailable)
                            {
                                var buffer = new byte[c.ReceiveBufferSize];
                                int readCnt = ns.Read(buffer, 0, buffer.Length);
                                string payload = Encoding.UTF8.GetString(buffer, 0, readCnt);
                                if (!PreventExceptionAndSendResponse(payload, ns))
                                {
                                    var request = JsonConvert.DeserializeObject<Request>(payload);
                                    await CreateResponse(request, ns);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //ToDo - log to file
                    //if (ex is JsonSerializationException)
                    //else 
                }
            }
            _Listener.Stop();
            return false;
        }

        

        /// <summary>  </summary>
        /// <param name="requestObj"></param>
        private async Task<bool> CreateResponse(Request requestObj, NetworkStream network)
        {
            string statTxt = StatusResponse.GetStatusCodeText(StatusResponse.STATUSCODE.BADREQUEST);  //ToDo
            if (ValidateIsNull(requestObj))
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.DEFAULT, ref statTxt);
            else if (string.IsNullOrEmpty(requestObj.Method))
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.METHOD, ref statTxt);
            else if (String.IsNullOrEmpty(requestObj.Body))
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.MISSINGBODY, ref statTxt);
            else if (!requestObj.ValidPath() && requestObj.Method != "echo" && requestObj.Path!="testing")
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.PATHRESOURSE, ref statTxt);
            else if (requestObj.Date <= 0)
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.DATE, ref statTxt);
            else if (!requestObj.ValidMethod())
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.ILLEGALMETHOD, ref statTxt);
            else if (!requestObj.ValidBody())
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.ILLEGALBODY, ref statTxt);
            else if (!requestObj.ValidDate())      // doesn't reach if Date is sent as a non long data type - test 6
                StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.ILLEGALDATE, ref statTxt);

            var bodyText = requestObj.Method == "echo" ? requestObj.Body : "";
            var response = new Response { Body = bodyText, Status = statTxt };
            await SendResponse(response, network);
            return true;
        }



        /// <summary>  </summary>
        /// <param name="requestObj"></param>
        private async Task<bool> SendResponse(Response responseObj, NetworkStream network)
        {
            var jsonObj = JsonConvert.SerializeObject(responseObj, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            byte[] msg = Encoding.UTF8.GetBytes(jsonObj);
            network.Write(msg, 0, msg.Length);
            return true;
        }


        /// <summary> Check if the Request object is completely empty of data </summary>
        /// <param name="requestObj"> an instance of the Request object from the client </param>
        private bool ValidateIsNull(Request requestObj)
        {
            if (requestObj == null)
                return true;
            else if (string.IsNullOrEmpty(requestObj.Body) && requestObj.Date == 0
                   && string.IsNullOrEmpty(requestObj.Method) && string.IsNullOrEmpty(requestObj.Path))
                return true;
            return false;
        }



        /// <summary>  </summary>
        /// <param name="payload"></param>
        /// <param name="ns"></param>
        private bool PreventExceptionAndSendResponse(string payload, NetworkStream ns)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<Request>(payload);
            }
            catch (Exception ex)
            {
                if (ex is JsonSerializationException)
                {
                    var splitter = ex.Message.Contains("Path 'date'");
                    string statTxt = StatusResponse.GetStatusCodeText(StatusResponse.STATUSCODE.ERROR);
                    StatusResponse.GetStatusCodeReasonText(StatusResponse.REQUESTERRORFIELD.ILLEGALDATE, ref statTxt);
                    var responseObj = new Response { Body = "", Status = statTxt };
                    SendResponse(responseObj, ns);
                }
                return false;
            }
            return true;
        }


    }
}