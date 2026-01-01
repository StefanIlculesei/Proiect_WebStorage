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

    public class PlanDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long MaxFileSize { get; set; }
        public long LimitSize { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int MaxFileCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  // active, canceled, expired, trialing
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public PlanDetailResponse? Plan { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public int PlanId { get; set; }
        public bool AutoRenew { get; set; } = true;
    }

    public class UpgradePlanRequest
    {
        public int PlanId { get; set; }
    }
}

