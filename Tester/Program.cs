using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing console app");
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
