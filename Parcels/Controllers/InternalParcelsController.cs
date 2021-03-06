using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parcels.Models;

namespace Parcels.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    public class InternalParcelsController : Controller
    {
        private const string ParcelsDictName = "parcels";
        private readonly IReliableStateManager stateManager;

        public InternalParcelsController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        // GET Parcels
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, Parcel> parcelsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Parcel>>(ParcelsDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Parcel>> list = await parcelsDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Parcel>> enumerator = list.GetAsyncEnumerator();

                List<Parcel> result = new List<Parcel>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current.Value);
                }

                return this.Json(result);
            }
        }

        // GET Parcels/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, Parcel> parcelsDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, Parcel>>(ParcelsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Parcel>> list = await parcelsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Parcel>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return new NotFoundResult();
            }
        }

        // POST Parcels/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string RequestId)
        {
            IReliableDictionary<string, Parcel> parcelsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Parcel>>(ParcelsDictName);

            Parcel newParcel = new Parcel { RequestId = RequestId, Status = ParcelStatus.WaitingForPickup };

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await parcelsDictionary.AddAsync(tx, RequestId, newParcel);
                await tx.CommitAsync();
            }

            return Json(newParcel);
        }

        // PATCH InternalParcels/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> ChangeStatus(string id, int Status)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, Parcel> parcelsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, Parcel>>(ParcelsDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Parcel>> list = await parcelsDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Parcel>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == id)
                    {
                        enumerator.Current.Value.Status = (ParcelStatus)Status;
                        await parcelsDictionary.AddOrUpdateAsync(tx, id, enumerator.Current.Value, (key, oldvalue) => enumerator.Current.Value);

                        return Json(enumerator.Current.Value);
                    }
                }

                return new NotFoundResult();
            }
        }
    }
}
