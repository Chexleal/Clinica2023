namespace clinicamhsystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public Guid? ErrorLogId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
