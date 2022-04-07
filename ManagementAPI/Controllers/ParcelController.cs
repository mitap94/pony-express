using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Parcels.Models;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManagementAPI.Controllers
{
    [Route("api/parcels")]
    public class ParcelController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext serviceContext;

        public ParcelController(HttpClient httpClient, StatelessServiceContext serviceContext, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.serviceContext = serviceContext;
        }

        // GET api/parcels
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalParcels 
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<Parcel> result = new List<Parcel>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl =
                    $"{proxyAddress}/InternalParcels?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController GET {proxyUrl}");

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController Failed");
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<Parcel>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        // GET api/parcels/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string Id)
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalParcels 
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalParcels/{Id}?PartitionKey={Utils.GetParcelsPartitionKeyFromId(Id)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController GET {proxyUrl}");

            using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController Failed");
                    return new ContentResult()
                    {
                        StatusCode = (int)response.StatusCode,
                    };
                }
                else
                {
                    return new ContentResult()
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = await response.Content.ReadAsStringAsync(),
                        ContentType = "application/json"
                    };
                }
            }
        }

        // POST api/parcels/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string RequestId)
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalParcels
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalParcels/create?RequestId={RequestId}&PartitionKey={Utils.GetParcelsPartitionKeyFromId(RequestId)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController POST {proxyUrl}");

            using (HttpResponseMessage response = await this.httpClient.PostAsync(proxyUrl, null))
            {
                return new ContentResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync(),
                    ContentType = "application/json"
                };
            }
        }

        // PATCH api/parcels/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatus(string id, int Status)
        {
            switch (Status)
            {
                case 1:
                    {
                        ManagementAPI.RegisterParcelPickupForMetrics();
                        break;
                    }
                case 2:
                    {
                        ManagementAPI.RegisterParcelDeliveryForMetrics();
                        break;
                    }
                case 3:
                    {
                        ManagementAPI.RegisterParcelFailedDeliveryForMetrics();
                        break;
                    }
                default:
                    {
                        ManagementAPI.RegisterGeneralRequestForMetrics();
                        break;
                    }
            }

            // Calling InternalParcels
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalParcels/{id}?Status={Status}" +
                $"&PartitionKey={Utils.GetParcelsPartitionKeyFromId(id)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController PATCH {proxyUrl}");

            using (HttpResponseMessage response = await this.httpClient.PatchAsync(proxyUrl, null))
            {
                return new ContentResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync(),
                    ContentType = "application/json"
                };
            }
        }
    }
}
