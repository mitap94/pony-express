using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requests.Models
{
    public class Request
    {
        public Request(int RequestId, int UserId, string Content, string FromLocation, string ToLocation, decimal Weight)
        {
            this.RequestId = RequestId;
            this.UserId = UserId;
            this.Content = Content;
            this.FromLocation = FromLocation;
            this.ToLocation = ToLocation;
            this.Weight = Weight;
            this.Status = RequestStatus.NotHandled;
        }

        private int RequestId { get; set; }
        private int UserId { get; set; }
        private string Content { get; set; }
        private string FromLocation { get; set; }
        private string ToLocation { get; set; }
        private decimal Weight { get; set; }
        private RequestStatus Status { get; set; }
    }
}
