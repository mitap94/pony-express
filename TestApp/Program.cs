using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tester.Mock;

namespace Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting test app...");

            Thread.Sleep(5000);
            Console.WriteLine();

            Console.WriteLine("Fill the app with 3 initial users and 3 initial requests");
            for (int i = 0; i < 3; i++)
            {
                await UserGenerator.CreateRandomUser();
                await RequestGenerator.CreateRandomRequest();
            }

            Thread.Sleep(5000);
            Console.WriteLine();

            Console.WriteLine("Starting random API testing");
            Thread.Sleep(5000);
            Console.WriteLine();
            while (true) {
                await PickRandomAPI();

                Console.WriteLine("Sleep 1s....");
                Thread.Sleep(1000);
            }
        }

        // Specify different weight for APIs
        // 2 - users API
        // 5 - requests API
        // 3 - parcels API
        static async Task PickRandomAPI()
        {
            switch(new Random().Next(10))
            {
                case 0:
                case 1:
                    await UserGenerator.PickRandomUserAPICall();
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    await RequestGenerator.PickRandomRequestAPICall();
                    break;
                case 7:
                case 8:
                case 9:
                    await ParcelGenerator.PickRandomParcelAPICall();
                    break;
            }
        }
    }
}
