namespace WebAPIClient.DTOs
{
    public class FileUploadRequest
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public int? FolderId { get; set; }
        public string Visibility { get; set; } = "private";
        public IFormFile? File { get; set; }
    }

    public class FileResponse
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? MimeType { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Visibility { get; set; }
        public int? FolderId { get; set; }
        public string? FolderName { get; set; }
    }

    public class FileUpdateRequest
    {
        public string? FileName { get; set; }
        public string? Visibility { get; set; }
        public int? FolderId { get; set; }
    }

    public class MoveFileRequest
    {
        public int? TargetFolderId { get; set; }
    }

    public class BulkMoveFilesRequest
    {
        public List<int> FileIds { get; set; } = new();
        public int? TargetFolderId { get; set; }
    }
}
