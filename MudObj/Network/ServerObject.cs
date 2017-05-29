using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MudObj.Network
{
    /// <summary>
    /// From Microsoft documentation.
    /// https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
    /// </summary>
    public class ServerObject
    {
        #region "Static declarations"

        public static ManualResetEvent IsAllDone = new ManualResetEvent(false);
        
        #endregion

        #region "Properties"

        public string IPAddressString { get; set; }
        public int PortNumber { get; set; }
        public Socket ListenerSocketObject { get; set; }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ServerObject()
        {
            AssignIPAddressAndPort(Constant.DEFAULT_IP_ADDRESS_STR, Constant.DEFAULT_PORT_NUMBER);
        }

        public ServerObject(string IPAddressString, int PortNumber)
        {
            AssignIPAddressAndPort(IPAddressString, PortNumber);
        }

        private void AssignIPAddressAndPort(string IPAddressString, int PortNumber)
        {
            this.IPAddressString = IPAddressString;
            this.PortNumber = PortNumber;
        }

        #endregion

        #region "Public Methods"

        public void Start()
        {
            IPAddress IPAddressObject = IPAddress.Parse(this.IPAddressString);
            IPEndPoint LocalEndPointObject = new IPEndPoint(IPAddressObject, this.PortNumber);
            ListenerSocketObject = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Bind to ipaddress and port.
                ListenerSocketObject.Bind(LocalEndPointObject);

                // Start listening.
                ListenerSocketObject.Listen(Constant.MAX_CONNECTIONS);

                while(true)
                {
                    IsAllDone.Reset();

                    this.Log("Waiting for connection...");

                    ListenerSocketObject.BeginAccept(new AsyncCallback(AcceptConnectionCallback), ListenerSocketObject);

                    IsAllDone.WaitOne();
                }
            }
            catch(Exception e)
            {
                // Generic error handler.
                ErrorHandler.ErrorHandler.ShowErroHandler(e);
            }
        }

        public void Log(string Message)
        {
            Console.WriteLine(Message);
        }

        #endregion

        #region "Callback handlers"

        public void AcceptConnectionCallback(IAsyncResult AsyncResult)
        {
            // Signal the main thread to continue.
            IsAllDone.Set();

            Socket ListenerSocket = (Socket)AsyncResult.AsyncState;
            Socket AcceptedSocket = ListenerSocket.EndAccept(AsyncResult);

            ConnectionObject ConnObj = new ConnectionObject();
            ConnObj.SocketObject = AcceptedSocket;
            ConnObj.IsNewConnection = true;

            AcceptedSocket.BeginReceive(ConnObj.ReceiveBuffer, 0, Constant.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), ConnObj);
            this.Log(String.Format("Connection from {0} accepted.", ConnObj.SocketObject.RemoteEndPoint.ToString()));
        }
        
        public void ReceiveDataCallback(IAsyncResult AsyncResult)
        {
            string content = String.Empty;

            ConnectionObject connObj = (ConnectionObject)AsyncResult.AsyncState;
            Socket FromThisSocket = connObj.SocketObject;

            int BytesRead = FromThisSocket.EndReceive(AsyncResult);

            if (BytesRead > 0)
            {
                if (connObj.IsNewConnection)
                {
                    // Sometimes new connections create some "received" data that doesn't make sense.
                    // So we need to discard these data received.
                    connObj.IsNewConnection = false;
                }
                else
                {
                    connObj.TempStringBuilder.Append(Encoding.UTF8.GetString(connObj.ReceiveBuffer, 0, BytesRead));
                    content = connObj.TempStringBuilder.ToString();

                    // Echo back when seeing CRLF.
                    if (content.IndexOf(Environment.NewLine) > -1)
                    {
                        this.Log(String.Format("Read {0} bytes from client.  Data: [{1}]", content.Length, content));
                        SendData(FromThisSocket, content);
                        connObj.TempStringBuilder.Clear();
                    }

                }

                // Always Keep reading.
                FromThisSocket.BeginReceive(connObj.ReceiveBuffer, 0, Constant.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), connObj);
            }
        }

        public void SendDataCallback(IAsyncResult AsyncResult)
        {
            try
            {
                // Retrieve stocket from the state object.
                Socket ToThisSocket = (Socket)AsyncResult.AsyncState;
                int BytesSent = ToThisSocket.EndSend(AsyncResult);
                this.Log(String.Format("Sent {0} byte(s) to client.", BytesSent));

                //ToThisSocket.Shutdown(SocketShutdown.Both);
                //ToThisSocket.Close();
            }
            catch(Exception e)
            {
                ErrorHandler.ErrorHandler.ShowErroHandler(e);
            }
        }

        public void SendData(Socket ToThisSocket, string Data)
        {
            byte[] ByteDataToSend = Encoding.UTF8.GetBytes(Data);
            ToThisSocket.BeginSend(ByteDataToSend, 0, ByteDataToSend.Length, SocketFlags.None, new AsyncCallback(SendDataCallback), ToThisSocket);
        }


        #endregion

    }
}
