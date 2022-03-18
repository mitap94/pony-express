using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parcels.Models;

namespace Parcels.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParcelController : Controller
    {
        private const string ParcelsDictName = "parcels";
        private static int ParcelIdCount = 0;
        private readonly IReliableStateManager stateManager;

        public ParcelController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        // GET api/Parcel
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, Parcel> parcelsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, Parcel>>(ParcelsDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, Parcel>> list = await parcelsDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, Parcel>> enumerator = list.GetAsyncEnumerator();

                List<KeyValuePair<int, Parcel>> result = new List<KeyValuePair<int, Parcel>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                return this.Json(result);
            }
        }

        // GET api/Parcel/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int Id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<int, Parcel> parcelsDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<int, Parcel>>(ParcelsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, Parcel>> list = await parcelsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, Parcel>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == Id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return new NotFoundResult();
            }
        }

        // POST api/Parcel/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(int RequestId)
        {
            IReliableDictionary<int, Parcel> parcelsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, Parcel>>(ParcelsDictName);

            string TrackingId = "PE" + ParcelIdCount;
            Parcel newParcel = new Parcel(TrackingId, RequestId);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await parcelsDictionary.AddAsync(tx, ParcelIdCount++, newParcel);
                await tx.CommitAsync();
            }

            //return Json(newParcel);
            return new OkResult();
        }
    }
}
