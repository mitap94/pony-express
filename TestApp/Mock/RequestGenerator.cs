using Newtonsoft.Json;
using Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tester.Mock
{
    class RequestGenerator
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly string RequestAPIEndpoint = "http://localhost:19081/PackageDelivery/ManagementAPI/api/requests/";

        public static async Task CreateRandomRequest()
        {
            Console.WriteLine($"CREATE REQUEST");

            string URL = RequestAPIEndpoint + $"create?UserId={Data.GetRandomUser().Id}&Content={Data.GetRandomContent()}&FromLocation={Data.GetRandomCity()}&ToLocation={Data.GetRandomCity()}&Weight={Data.GetRandomWeight()}";

            Console.WriteLine($"Started create request... URL = {URL}");

            using (HttpResponseMessage response = await httpClient.PostAsync(URL, null))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"RequestGenerator CreateRequest failed, status code: {response.StatusCode}, { await response.Content.ReadAsStringAsync()}");
                    return;
                }

                Console.WriteLine("Deserialize");
                Request newRequest = JsonConvert.DeserializeObject<Request>(await response.Content.ReadAsStringAsync());
                Console.WriteLine($"RequestGenerator CreateRequest {newRequest.RequestId}, {newRequest.UserId}, {newRequest.Content}, {newRequest.FromLocation}, {newRequest.ToLocation}, {newRequest.Weight}");

                Data.PendingRequestList.Add(newRequest);
                Console.WriteLine($"Request list size: {Data.PendingRequestList.Count}");
            }
        }

        public static async Task GetAllRequests()
        {
            Console.WriteLine($"GET ALL REQUESTS");

            string URL = RequestAPIEndpoint;
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"RequestGenerator GetAllRequests failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        public static async Task GetRequest(string id)
        {
            Console.WriteLine($"GET REQUEST {id}");

            string URL = RequestAPIEndpoint + $"{id}";
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"RequestGenerator GetRequest failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        public static async Task ChangeStatus(string id, int status)
        {
            Console.WriteLine($"CHANGE REQUEST STATUS {id} {status}");

            string URL = RequestAPIEndpoint + $"{id}?Status={status}";
            using (HttpResponseMessage response = await httpClient.PatchAsync(URL, null))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"RequestGenerator ChangeStatus failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        // Specify different weight for calls
        // 4 - CreateRandomRequest
        // 1 - GetAllRequests
        // 2 - GetRequest
        // 3 - ChangeStatus
        public static async Task PickRandomRequestAPICall()
        {
            switch (new Random().Next(10))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    await CreateRandomRequest();
                    break;
                case 4:
                    await GetAllRequests();
                    break;
                case 5:
                case 6:
                    if (Data.PendingRequestList.Count > 0) {
                        await GetRequest(Data.GetRandomRequest().RequestId);
                    } else
                    {
                        await GetAllRequests();
                    }
                    break;
                case 7:
                case 8:
                case 9:
                    if (Data.PendingRequestList.Count <= 0)
                    {
                        await GetAllRequests();
                        return;
                    }
                 
                    Request req = Data.GetRandomRequest();

                    switch (new Random().Next(4))
                    {
                        case 0:
                            // Deny request
                            await ChangeStatus(req.RequestId, (int) RequestStatus.Denied);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            // Handle request -> create parcel
                            await ChangeStatus(req.RequestId, (int) RequestStatus.Handled);
                            await ParcelGenerator.CreateParcel(req.RequestId);
                            break;
                    }

                    // We remove both denied and handled requests from the internal list
                    // as we won't be using them anymore for any test calls
                    Data.PendingRequestList.Remove(req);

                    break;
            }
        }
    }
}
