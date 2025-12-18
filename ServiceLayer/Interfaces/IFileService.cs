using System.Collections.Generic;
using System.Threading.Tasks;
using ModelLibrary.Models;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Interfaces;

public interface IFileService
{
    Task<FileModel?> GetByIdAsync(int id, int userId);
    Task<IReadOnlyList<FileModel>> GetByUserIdAsync(int userId);
    Task<IReadOnlyList<FileModel>> GetByFolderIdAsync(int folderId);
    Task<IReadOnlyList<FileModel>> SearchByNameAsync(int userId, string searchTerm);
    Task<IReadOnlyList<FileModel>> GetRecentFilesAsync(int userId, int limit = 10);
    Task<FileModel> UploadFileAsync(int userId, int? folderId, string fileName, long fileSize, string storagePath, string? mimeType, string? visibility);
    Task<FileModel> UpdateFileAsync(int id, int userId, string? fileName = null, string? visibility = null, int? folderId = null);
    Task<bool> DeleteFileAsync(int id, int userId);
    Task<FileModel?> MoveFileAsync(int id, int userId, int? targetFolderId);
    Task<int> BulkMoveFilesAsync(List<int> fileIds, int userId, int? targetFolderId);
    Task<IReadOnlyList<FileEvent>> GetEventsForFileAsync(int fileId);
}