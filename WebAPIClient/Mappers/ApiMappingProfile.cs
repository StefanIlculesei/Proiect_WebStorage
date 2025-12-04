using AutoMapper;
using ModelLibrary.Models;
using WebAPIClient.DTOs;

namespace WebAPIClient.Mappers
{
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            // User mappings
            CreateMap<User, UserProfileResponse>()
                .ForMember(dest => dest.StorageUsed, opt => opt.MapFrom(src => src.StorageUsed));
            
            CreateMap<UpdateProfileRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StorageUsed, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Plan mappings
            CreateMap<Plan, PlanResponse>();

            // Subscription mappings
            CreateMap<Subscription, SubscriptionResponse>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.Name))
                .ForMember(dest => dest.StorageLimit, opt => opt.MapFrom(src => src.Plan.LimitSize));

            // Folder mappings
            CreateMap<Folder, FolderResponse>();
            CreateMap<Folder, FolderTreeResponse>()
                .ForMember(dest => dest.SubFolders, opt => opt.MapFrom(src => src.SubFolders));
            CreateMap<Folder, FolderContentsResponse>()
                .ForMember(dest => dest.SubFolders, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files));
            CreateMap<FolderRequest, Folder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // File mappings
            CreateMap<ModelLibrary.Models.File, FileResponse>()
                .ForMember(dest => dest.FolderName, opt => opt.MapFrom(src => src.Folder != null ? src.Folder.Name : null));
            CreateMap<FileUpdateRequest, ModelLibrary.Models.File>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore())
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())
                .ForMember(dest => dest.MimeType, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Checksum, opt => opt.Ignore());
        }
    }
}
