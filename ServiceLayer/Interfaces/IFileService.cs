using System.Collections.Generic;
using System.Threading.Tasks;
using ModelLibrary.Models;
using FileModel = ModelLibrary.Models.File;

namespace ServiceLayer.Interfaces;

public interface IFileService
{
    Task<FileModel?> GetByIdAsync(int id);
    Task<IReadOnlyList<FileModel>> ListByFolderAsync(int folderId);
    Task<FileModel> CreateAsync(FileModel file);
    Task<FileModel> UpdateAsync(FileModel file);
    Task<bool> DeleteAsync(int id);
    Task<IReadOnlyList<FileEvent>> GetEventsForFileAsync(int fileId);
}