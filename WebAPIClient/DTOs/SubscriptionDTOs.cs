namespace WebAPIClient.DTOs
{
    public class PlanResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long StorageLimit { get; set; }
        public long MaxFileSize { get; set; }
        public string? BillingPeriod { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public long StorageLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public bool AutoRenew { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class CreateSubscriptionRequest
    {
        public int PlanId { get; set; }
        public bool AutoRenew { get; set; } = true;
    }
}
