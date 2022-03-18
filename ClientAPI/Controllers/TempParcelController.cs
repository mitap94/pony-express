using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientAPI.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class TempParcelController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly StatelessServiceContext serviceContext;

        public TempParcelController(HttpClient httpClient, StatelessServiceContext context, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.serviceContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Caling Parcels 
            Uri serviceName = GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = this.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<WeatherForecast> result = new List<WeatherForecast>();

            foreach (Partition partition in partitions)
            {
                //{((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}
                string proxyUrl =
                    $"{proxyAddress}/WeatherForecast?PartitionKey=0&PartitionKind=Int64Range";

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<WeatherForecast>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        private Uri GetProxyAddress(Uri serviceName)
        {
            return new Uri($"http://localhost:19081{serviceName.AbsolutePath}");
        }

        private long GetPartitionKey(string name)
        {
            return 0;
        }

        internal static Uri GetParcelsServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/Parcels");
        }
    }
}
