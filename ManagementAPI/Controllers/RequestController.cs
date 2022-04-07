using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Requests.Models;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManagementAPI.Controllers
{
    [Route("api/requests")]
    public class RequestController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext serviceContext;

        public RequestController(HttpClient httpClient, StatelessServiceContext serviceContext, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.serviceContext = serviceContext;
        }

        // GET api/requests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalRequests 
            Uri serviceName = Utils.GetRequestsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<Request> result = new List<Request>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl =
                    $"{proxyAddress}/InternalRequests?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController GET {proxyUrl}");

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController Failed");
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<Request>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        // GET api/requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string Id)
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalRequests 
            Uri serviceName = Utils.GetRequestsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalRequests/{Id}?PartitionKey={Utils.GetRequestsPartitionKeyFromId(Id)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController GET {proxyUrl}");

            using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController Failed");
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

        // POST api/requests/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string UserId, string Content, string FromLocation, string ToLocation, decimal Weight)
        {
            ManagementAPI.RegisterParcelRequestCreationForMetrics();

            // Calling InternalRequests
            Uri serviceName = Utils.GetRequestsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalRequests/create?UserId={UserId}&Content={Content}&FromLocation={FromLocation}&ToLocation={ToLocation}&Weight={Weight}" +
                $"&PartitionKey={Utils.GetRequestsPartitionKeyFromCity(FromLocation)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController POST {proxyUrl}");

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

        // PATCH api/requests/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatus(string id, int Status)
        {
            switch (Status)
            {
                case 1:
                    {
                        ManagementAPI.RegisterParcelRequestDenialForMetrics();
                        break;
                    }
                case 2:
                    {
                        ManagementAPI.RegisterParcelRequestApprovalForMetrics();
                        break;
                    }
                default:
                    {
                        ManagementAPI.RegisterGeneralRequestForMetrics();
                        break;
                    }
            }

            // Calling InternalRequests
            Uri serviceName = Utils.GetRequestsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalRequests/{id}?Status={Status}" +
                $"&PartitionKey={Utils.GetRequestsPartitionKeyFromId(id)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"RequestController PATCH {proxyUrl}");

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
