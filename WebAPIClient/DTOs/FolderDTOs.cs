namespace WebAPIClient.DTOs
{
    public class FolderRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
    }

    public class FolderResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int FileCount { get; set; }
        public int SubFolderCount { get; set; }
    }

    public class FolderTreeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public List<FolderTreeResponse> SubFolders { get; set; } = new();
        public int FileCount { get; set; }
    }

    public class FolderContentsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public List<FolderResponse> SubFolders { get; set; } = new();
        public List<FileResponse> Files { get; set; } = new();
    }

    public class MoveFolderRequest
    {
        public int? TargetParentFolderId { get; set; }
    }
}
