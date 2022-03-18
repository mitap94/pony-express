using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parcels.Models
{
    public class Parcel
    {
        public Parcel(string TrackingId, int RequestId)
        {
            this.TrackingId = TrackingId;
            this.RequestId = RequestId;
            this.Status = ParcelStatus.WaitingForPickup;
        }

        private string TrackingId { get; set; }
        private int RequestId { get; set; }
        private ParcelStatus Status { get; set; }
    }
}
