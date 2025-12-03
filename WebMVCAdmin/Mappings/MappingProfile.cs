using AutoMapper;
using ModelLibrary.Models;
using WebMVC_Plans.Models;

namespace WebMVCAdmin.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Plan Mappings
            CreateMap<Plan, PlanViewModel>()
                .ForMember(dest => dest.LimitSizeGB, opt => opt.MapFrom(src => src.LimitSize / (1024.0 * 1024.0 * 1024.0)))
                .ForMember(dest => dest.MaxFileSizeMB, opt => opt.MapFrom(src => src.MaxFileSize / (1024.0 * 1024.0)))
                .ForMember(dest => dest.SubscriptionCount, opt => opt.Ignore()); // Handled manually or via separate query

            CreateMap<PlanViewModel, Plan>()
                .ForMember(dest => dest.LimitSize, opt => opt.MapFrom(src => (long)(src.LimitSizeGB * 1024 * 1024 * 1024)))
                .ForMember(dest => dest.MaxFileSize, opt => opt.MapFrom(src => (long)(src.MaxFileSizeMB * 1024 * 1024)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Handled by controller/db
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Handled by controller
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore());

            // Subscription Mappings
            CreateMap<Subscription, SubscriptionViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : $"User #{src.UserId}"))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name : $"Plan #{src.PlanId}"));

            CreateMap<CreateSubscriptionViewModel, Subscription>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore())
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());
        }
    }
}
