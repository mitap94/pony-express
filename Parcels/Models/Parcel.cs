using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parcels.Models
{
    public class Parcel
    {
        public string RequestId { get; set; }
        public ParcelStatus Status { get; set; }
    }
}
