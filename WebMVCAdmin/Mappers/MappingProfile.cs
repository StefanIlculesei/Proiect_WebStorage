using AutoMapper;
using ModelLibrary.Models;
using WebMVC_Plans.Models;

namespace WebMVCAdmin.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Plan -> PlanViewModel
            CreateMap<Plan, PlanViewModel>()
                .ForMember(dest => dest.LimitSizeGB, opt => opt.MapFrom(src => src.LimitSize / (1024.0 * 1024.0 * 1024.0)))
                .ForMember(dest => dest.MaxFileSizeMB, opt => opt.MapFrom(src => src.MaxFileSize / (1024.0 * 1024.0)))
                .ForMember(dest => dest.SubscriptionCount, opt => opt.MapFrom(src => src.Subscriptions != null ? src.Subscriptions.Count : 0));

            // PlanViewModel -> Plan
            CreateMap<PlanViewModel, Plan>()
                .ForMember(dest => dest.LimitSize, opt => opt.MapFrom(src => (long)(src.LimitSizeGB * 1024 * 1024 * 1024)))
                .ForMember(dest => dest.MaxFileSize, opt => opt.MapFrom(src => (long)(src.MaxFileSizeMB * 1024 * 1024)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Usually set on creation
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Usually set on update

            // Subscription -> SubscriptionViewModel
            CreateMap<Subscription, SubscriptionViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : $"User #{src.UserId}"))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name : $"Plan #{src.PlanId}"));

            // CreateSubscriptionViewModel -> Subscription
            CreateMap<CreateSubscriptionViewModel, Subscription>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // User -> UserViewModel
            CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.StorageUsedBytes, opt => opt.MapFrom(src => src.StorageUsed))
                .ForMember(dest => dest.StorageUsedFormatted, opt => opt.MapFrom(src => FormatBytes(src.StorageUsed)))
                .ForMember(dest => dest.SubscriptionCount, opt => opt.MapFrom(src => src.Subscriptions != null ? src.Subscriptions.Count : 0));

            // User -> EditUserViewModel
            CreateMap<User, EditUserViewModel>();

            // EditUserViewModel -> User
            CreateMap<EditUserViewModel, User>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }
}
