using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MudObj.Network
{
    public class ConnectionObject
    {
        public Socket SocketObject { get; set; }
        public byte[] ReceiveBuffer { get; }
        public StringBuilder TempStringBuilder { get; }
        public bool IsNewConnection { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConnectionObject()
        {
            ReceiveBuffer = new byte[Constant.BUFFER_SIZE];
            TempStringBuilder = new StringBuilder();
        }
    }
}
