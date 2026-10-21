using AutoMapper;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Api.Mapping
{
    /// <summary>
    /// نگاشت Entity ↔ DTOهای لایه‌ی وب.
    /// این پروفایل در اسمبلی Api است تا وابستگی معکوس WebFramework → Api ایجاد نشود؛
    /// InitializeAutoMapper هر دو اسمبلی را اسکن می‌کند.
    /// </summary>
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            // User → UserSelectDto (پروفایل و نقش‌ها از Navigationها خوانده می‌شوند)
            CreateMap<User, UserSelectDto>()
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.UserProfile.FirstOrDefault() != null ? src.UserProfile.FirstOrDefault()!.FirstName : null))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.UserProfile.FirstOrDefault() != null ? src.UserProfile.FirstOrDefault()!.LastName : null))
                .ForMember(dest => dest.NationalCode,
                    opt => opt.MapFrom(src => src.UserProfile.FirstOrDefault() != null ? src.UserProfile.FirstOrDefault()!.NationalCode : null))
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.RoleNameFa)));

            // CreateUserDto → User (رمز، نقش و پروفایل در سرویس ساخته می‌شوند)
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
                .ForMember(dest => dest.UserSessions, opt => opt.Ignore());
        }
    }
}