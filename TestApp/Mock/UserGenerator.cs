using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Users.Models;

namespace Tester.Mock
{
    class UserGenerator
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly string UserAPIEndpoint = "http://localhost:19081/PackageDelivery/ManagementAPI/api/users/";

        public static async Task CreateRandomUser()
        {
            string URL = UserAPIEndpoint + $"create?Name={Data.GetRandomName()}&City={Data.GetRandomCity()}";

            using (HttpResponseMessage response = await httpClient.PostAsync(URL, null))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"UserGenerator CreateRandomUser failed, status code: {response.StatusCode}, { await response.Content.ReadAsStringAsync()}");
                    return;
                }

                User newUser = JsonConvert.DeserializeObject<User> (await response.Content.ReadAsStringAsync());
                Console.WriteLine($"CREATE USER {newUser.Id}, {newUser.Name}, {newUser.City}");

                Data.CreatedUserList.Add(newUser);
                Console.WriteLine($"User list size: {Data.CreatedUserList.Count}");
            }
        }

        public static async Task GetAllUsers()
        {
            Console.WriteLine($"GET ALL USERS");

            string URL = UserAPIEndpoint;
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine( $"UserGenerator GetAllUsers failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        public static async Task GetUser(string id)
        {
            Console.WriteLine($"GET USER {id}");

            string URL = UserAPIEndpoint + $"{id}";
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"UserGenerator GetUser failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        // Specify different weight for calls
        // 3 - CreateRandomUser
        // 6 - GetUser
        // 1 - GetAllUsers
        public static async Task PickRandomUserAPICall()
        {
            switch(new Random().Next(10))
            {
                case 0:
                case 1:
                case 2:
                    await CreateRandomUser();
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    await GetUser(Data.GetRandomUser().Id);
                    break;
                case 9:
                    await GetAllUsers();
                    break;
            }
        }
    }
}
