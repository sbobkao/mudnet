using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudObj.ErrorHandler
{
    public static class ErrorHandler
    {
        public static void ShowErroHandler(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
