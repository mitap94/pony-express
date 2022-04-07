namespace Requests.Models
{
    public class Request
    {
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public decimal Weight { get; set; }
        public RequestStatus Status { get; set; }
    }
}
