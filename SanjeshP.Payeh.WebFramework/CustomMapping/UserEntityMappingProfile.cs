using AutoMapper;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.WebFramework.CustomMapping
{
    /// <summary>
    /// پروفایل نگاشت کاربر: Entity ↔ DTO.
    /// نکته: DTOها در لایه‌ی Api تعریف می‌شوند؛ برای اینکه WebFramework به Api وابسته نشود
    /// (وابستگی معکوس ممنوع)، نگاشت‌های مربوط به DTOهای وب در پروفایل لایه‌ی Api
    /// یعنی ApiMappingProfile ثبت می‌شوند و این پروفایل فقط نگاشت‌های سطح موجودیت را دارد.
    /// </summary>
    public class UserEntityMappingProfile : Profile
    {
        public UserEntityMappingProfile()
        {
            // نگاشت ویرایشی: فقط مقادیر غیر null روی موجودیت اعمال می‌شوند (PATCH-like).
            CreateMap<User, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserSessions, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember is not null));
        }
    }
}