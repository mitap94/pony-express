using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private const string usersDictName = "users";
        private static int UserIdCount = 0;
        private readonly IReliableStateManager stateManager;

        public UserController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        

        // GET api/User
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            CancellationToken ct = new CancellationToken();

            IReliableDictionary<int, User> usersDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, User>>(usersDictName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, User>> list = await usersDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, User>> enumerator = list.GetAsyncEnumerator();

                List<KeyValuePair<int, User>> result = new List<KeyValuePair<int, User>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                return this.Json(result);
            }
        }

        // GET api/User/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int Id)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<int, User> usersDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<int, User>>(usersDictName);

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<int, User>> list = await usersDictionary.CreateEnumerableAsync(tx);
                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<int, User>> enumerator = list.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(ct))
                {
                    if (enumerator.Current.Key == Id)
                        return new JsonResult(enumerator.Current.Value);
                }

                return new NotFoundResult();
            }
        }

        // POST api/User/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(string Name, string City)
        {
            IReliableDictionary<int, User> usersDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<int, User>>(usersDictName);

            User newUser = new User(UserIdCount, Name, City);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await usersDictionary.AddAsync(tx, UserIdCount++, newUser);
                await tx.CommitAsync();
            }

            //return Json(newUser);
            return new OkResult();
        }
    }
}
