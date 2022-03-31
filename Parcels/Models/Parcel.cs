using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parcels.Models
{
    public class Parcel
    {
        public int RequestId { get; set; }
        public ParcelStatus Status { get; set; }
    }
}
