namespace SecureApiTemplate.Models.Core
{
    public class ErrorInfo
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public long Status { get; set; }
        public string TraceId { get; set; }
    }
}
