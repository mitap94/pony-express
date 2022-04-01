using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Users.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public UserType Type { get; set; }
    }
}
