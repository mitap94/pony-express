using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Users.Models;
using Common;

namespace ManagementAPI.Controllers
{
    [Route("api/users")]
    public class ClientController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext serviceContext;

        public ClientController(HttpClient httpClient, StatelessServiceContext serviceContext, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.serviceContext = serviceContext;
        }

        // GET api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalUsers 
            Uri serviceName = Utils.GetUsersServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<User> result = new List<User>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl =
                    $"{proxyAddress}/InternalUsers?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController get all addresses {proxyUrl}");

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController Failed");
                        continue;
                    }

                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController Successful");
                    result.AddRange(JsonConvert.DeserializeObject<List<User>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            ManagementAPI.RegisterGeneralRequestForMetrics();

            // Calling InternalUsers 
            Uri serviceName = Utils.GetUsersServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalUsers/{id}?PartitionKey={Utils.GetUsersPartitionKeyFromId(id)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController get address {proxyUrl}");

            using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController Failed");
                    return new ContentResult()
                    {
                        StatusCode = (int)response.StatusCode,
                    };
                } else
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController Succesful returning Content result, status code = {response.StatusCode}");
                    return new ContentResult()
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = await response.Content.ReadAsStringAsync(),
                        ContentType = "application/json"
                    };
                }
            }
        }

        // POST api/users/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string Name, string City, int Type=0)
        {
            ManagementAPI.RegisterUserCreationsForMetrics();

            // Calling InternalUsers 
            Uri serviceName = Utils.GetUsersServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalUsers/create?Name={Name}&City={City}&Type={Type}&PartitionKey={Utils.GetUsersPartitionKeyFromCity(City)}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ClientController create address {proxyUrl}");

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

    }
}
