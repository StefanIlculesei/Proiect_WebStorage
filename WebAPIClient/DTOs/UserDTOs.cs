namespace WebAPIClient.DTOs
{
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long StorageUsed { get; set; }
        public long StorageLimit { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }

    public class StorageUsageResponse
    {
        public long StorageUsed { get; set; }
        public long StorageLimit { get; set; }
        public double UsagePercentage { get; set; }
        public int TotalFiles { get; set; }
        public int TotalFolders { get; set; }
    }

    public class DashboardStatsResponse
    {
        public long StorageUsed { get; set; }
        public long StorageLimit { get; set; }
        public int StoragePercentage { get; set; }
        public int TotalFiles { get; set; }
        public int TotalFolders { get; set; }
    }
}
