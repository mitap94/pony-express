using System;
using System.Threading;
using System.Threading.Tasks;
using Tester.Mock;

namespace Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting TestApp...");

            Thread.Sleep(3000);
            Console.WriteLine();

            Console.WriteLine("Fill the app with 3 initial users and 3 initial requests");
            for (int i = 0; i < 3; i++)
            {
                await UserGenerator.CreateRandomUser();
                await RequestGenerator.CreateRandomRequest();
            }

            Console.WriteLine();
            Console.WriteLine();
            Thread.Sleep(3000);

            Console.WriteLine("Starting random API testing");
            Thread.Sleep(3000);
            Console.WriteLine();
            while (true) {
                await PickRandomAPI();

                Console.WriteLine();
                Console.WriteLine(".......");
                Thread.Sleep(1000);
                Console.WriteLine();
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
