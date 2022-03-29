﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Requests.Models;

namespace Requests.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RequestsController : Controller
    {
        private const string RequestsDictName = "requests";
        private static int RequestIdCount = 0;
        private readonly IReliableStateManager stateManager;

        public RequestsController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        // GET Requests
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, Request> requestsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, Request>>(RequestsDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, Request>> list = await requestsDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, Request>> enumerator = list.GetAsyncEnumerator();

                List<KeyValuePair<int, Request>> result = new List<KeyValuePair<int, Request>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                return this.Json(result);
            }
        }

        // GET Requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int Id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<int, Request> requestsDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<int, Request>>(RequestsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, Request>> list = await requestsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, Request>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == Id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return new NotFoundResult();
            }
        }

        // POST Requests/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(int UserId, string Content, string FromLocation, string ToLocation, decimal Weight)
        {
            IReliableDictionary<int, Request> requestsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, Request>>(RequestsDictName);

            Request newRequest = new Request { RequestId = RequestIdCount, UserId = UserId, Content = Content, FromLocation = FromLocation, ToLocation = ToLocation, Weight = Weight };

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await requestsDictionary.AddAsync(tx, RequestIdCount++, newRequest);
                await tx.CommitAsync();
            }

            return Json(newRequest);
        }
    }
}