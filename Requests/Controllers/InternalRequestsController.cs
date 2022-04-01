using Microsoft.AspNetCore.Mvc;
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
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    public class InternalRequestsController : Controller
    {
        private const string RequestsDictName = "requests";
        private static int RequestIdCount = 0;
        private readonly IReliableStateManager stateManager;

        public InternalRequestsController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        // GET InternalRequests
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, Request> requestsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Request>>(RequestsDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Request>> list = await requestsDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Request>> enumerator = list.GetAsyncEnumerator();

                List<Request> result = new List<Request>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current.Value);
                }

                return this.Json(result);
            }
        }

        // GET InternalRequests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string Id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, Request> requestsDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, Request>>(RequestsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Request>> list = await requestsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Request>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == Id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return new NotFoundResult();
            }
        }

        // POST InternalRequests/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string UserId, string Content, string FromLocation, string ToLocation, decimal Weight)
        {
            IReliableDictionary<string, Request> requestsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Request>>(RequestsDictName);

            string newId = "R" + FromLocation[0] + RequestIdCount++;

            Request newRequest = new Request { RequestId = newId, UserId = UserId, Content = Content, FromLocation = FromLocation, ToLocation = ToLocation, Weight = Weight, Status = RequestStatus.NotHandled };

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await requestsDictionary.AddAsync(tx, newId, newRequest);
                await tx.CommitAsync();
            }

            return Json(newRequest);
        }

        // PATCH InternalRequests/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatus(string id, int Status)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, Request> requestsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Request>>(RequestsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Request>> list = await requestsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Request>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == id) {
                        enumerator.Current.Value.Status = (RequestStatus) Status;
                        await requestsDictionary.AddOrUpdateAsync(tx, id, enumerator.Current.Value, (key, oldvalue) => enumerator.Current.Value);

                        return Json(enumerator.Current.Value);
                    }
                }

                return new NotFoundResult();
            }
        }
    }
}
