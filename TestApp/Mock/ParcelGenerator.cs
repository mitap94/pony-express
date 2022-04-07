using Newtonsoft.Json;
using Parcels.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tester.Mock
{
    class ParcelGenerator
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly string ParcelAPIEndpoint = "http://localhost:19081/PackageDelivery/ManagementAPI/api/parcels/";

        public static async Task CreateParcel(string RequestId)
        {
            Console.WriteLine($"CREATE PARCEL {RequestId}");

            string URL = ParcelAPIEndpoint + $"create?RequestId={RequestId}";

            using (HttpResponseMessage response = await httpClient.PostAsync(URL, null))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"ParcelGenerator CreateParcel failed, status code: {response.StatusCode}, { await response.Content.ReadAsStringAsync()}");
                    return;
                }

                Parcel newParcel = JsonConvert.DeserializeObject<Parcel>(await response.Content.ReadAsStringAsync());

                Data.PendingParcelList.Add(newParcel);
                Console.WriteLine($"Pending parcel list size: {Data.PendingParcelList.Count}");
            }
        }

        public static async Task GetAllParcels()
        {
            Console.WriteLine($"GET ALL PARCELS");

            string URL = ParcelAPIEndpoint;
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"ParcelGenerator GetAllParcels failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        public static async Task GetParcel(string id)
        {
            Console.WriteLine($"GET PARCEL {id}");

            string URL = ParcelAPIEndpoint + $"{id}";
            using (HttpResponseMessage response = await httpClient.GetAsync(URL))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"ParcelGenerator GetParcel failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        public static async Task ChangeStatus(string id, int status)
        {
            Console.WriteLine($"CHANGE PARCEL STATUS {id} {status}");

            string URL = ParcelAPIEndpoint + $"{id}?Status={status}";
            using (HttpResponseMessage response = await httpClient.PatchAsync(URL, null))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"ParcelGenerator ChangeStatus failed, status code: {response.StatusCode}, {response.Content.ReadAsStringAsync()}");
                    return;
                }
            }
        }

        // Specify different weight for calls
        // 0 - CreateParcel is done automatically when a request is created
        // 1 - GetAllParcels
        // 4 - GetParcel
        // 5 - ChangeStatus
        public static async Task PickRandomParcelAPICall()
        {
            switch (new Random().Next(10))
            {
                case 0: // GetAllParcels
                    await GetAllParcels();
                    break;
                case 1:
                case 2:
                case 3:
                case 4: // GetParcel
                    if (Data.PendingParcelList.Count > 0) {
                        await GetParcel(Data.GetRandomParcel().RequestId);
                    } else
                    {
                        await GetAllParcels();
                    }
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9: // Change Status
                    if (Data.PendingParcelList.Count <= 0)
                    {
                        await GetAllParcels();
                        return;
                    }

                    Parcel parcel = Data.GetRandomParcel();

                    // Fail 1/4 parcels
                    switch (new Random().Next(4))
                    {
                        case 0:
                            // Failed delivery
                            await ChangeStatus(parcel.RequestId, (int)ParcelStatus.FailedDelivery);

                            // We remove successful or failed delivery from internal list
                            // as we won't be using them anymore for any test calls
                            Data.PendingParcelList.Remove(parcel);

                            break;
                        case 1:
                        case 2:
                        case 3:
                            // Progress to next 
                            if (parcel.Status == ParcelStatus.WaitingForPickup)
                            {
                                await ChangeStatus(parcel.RequestId, (int) ParcelStatus.InTransit);
                                parcel.Status = ParcelStatus.InTransit; // also change locally
                            } else if (parcel.Status == ParcelStatus.InTransit)
                            {
                                await ChangeStatus(parcel.RequestId, (int) ParcelStatus.SuccessfulDelivery);

                                // We remove successful or failed delivery from internal list
                                // as we won't be using them anymore for any test calls
                                Data.PendingParcelList.Remove(parcel);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}
