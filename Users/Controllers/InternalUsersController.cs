using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    public class InternalUsersController : Controller
    {
        private const string usersDictName = "users";
        private readonly IReliableStateManager stateManager;
        private readonly StatefulServiceContext serviceContext;

        public InternalUsersController(StatefulServiceContext serviceContext, IReliableStateManager stateManager)
        {
            this.serviceContext = serviceContext;
            this.stateManager = stateManager;
        }

        // GET Users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, User> usersDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(usersDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, User>> list = await usersDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, User>> enumerator = list.GetAsyncEnumerator();

                List<User> result = new List<User>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current.Value);
                }

                return Json(result);
            }
        }

        // GET Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string Id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, User> usersDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(usersDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, User>> list = await usersDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, User>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == Id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return NotFound();
            }
            
        }

        // POST Users/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string Name, string City, int Type)
        {
            IReliableDictionary<string, User> usersDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(usersDictName);

            string newId = "U" + City[0] + "-" + Guid.NewGuid().ToString();

            User newUser = new User { Id = newId, Name = Name, City = City, Type = (UserType) Type };

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await usersDictionary.AddAsync(tx, newId, newUser);
                await tx.CommitAsync();
            }

            return Json(newUser);
        }
    }
}
