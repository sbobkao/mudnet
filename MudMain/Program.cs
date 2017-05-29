using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudObj.Network;

namespace MudMain
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerObject MudServer = new ServerObject();

            MudServer.Start();

        }
    }
}
