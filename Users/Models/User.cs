using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.Models
{
    public class User
    {
        public User(int Id, string Name, string City)
        {
            this.Id = Id;
            this.Name = Name;
            this.City = City;
        }
        private int Id { get; set; }
        private string Name { get; set; }
        private string City { get; set; }
    }
}
