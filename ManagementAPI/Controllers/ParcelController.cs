﻿using Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Parcels.Models;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
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
            // Calling Parcels 
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<Parcel> result = new List<Parcel>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl =
                    $"{proxyAddress}/InternalParcels?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController get all addresses {proxyUrl}");

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController Failed");
                        continue;
                    }

                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController Successful");
                    result.AddRange(JsonConvert.DeserializeObject<List<Parcel>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        // GET api/parcels/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int Id)
        {
            // Calling Parcels 
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            //{((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}
            string proxyUrl =
                $"{proxyAddress}/InternalParcels/{Id}?PartitionKey={Utils.GetParcelsPartitionKey()}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController get address {proxyUrl}");

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
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController Succesful returning Content result, status code = {response.StatusCode}");
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
        public async Task<IActionResult> Create(int RequestId)
        {
            // Calling Parcels
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalParcels/create?RequestId={RequestId}&PartitionKey={Utils.GetParcelsPartitionKey()}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController create address {proxyUrl}");

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
        public async Task<IActionResult> ChangeStatus(int id, int Status)
        {
            // Calling Parcels
            Uri serviceName = Utils.GetParcelsServiceName(this.serviceContext);
            Uri proxyAddress = Utils.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/InternalParcels/{id}?Status={Status}" +
                $"&PartitionKey={Utils.GetParcelsPartitionKey()}&PartitionKind=Int64Range";

            ServiceEventSource.Current.ServiceMessage(serviceContext, $"ParcelController create address {proxyUrl}");

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