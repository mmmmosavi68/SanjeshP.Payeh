using AutoMapper;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Api.Mapping
{
    /// <summary>
    /// نگاشت Entity و DTOهای لایه API.
    /// </summary>
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            CreateMap<User, UserSelectDto>()
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.FirstName
                            : null))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.LastName
                            : null))
                .ForMember(dest => dest.FatherName,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.FatherName
                            : null))
                .ForMember(dest => dest.NationalCode,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.NationalCode
                            : null))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.Gender
                            : null))
                .ForMember(dest => dest.MaritalStatus,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.MaritalStatus
                            : null))
                .ForMember(dest => dest.Religion,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.Religion
                            : null))
                .ForMember(dest => dest.LeftHanded,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.LeftHanded
                            : null))
                .ForMember(dest => dest.Nationality,
                    opt => opt.MapFrom(src =>
                        src.UserProfile.FirstOrDefault() != null
                            ? src.UserProfile.FirstOrDefault()!.Nationality
                            : null))
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.UserRoles
                        .Where(ur => ur.Role != null)
                        .Select(ur => new RoleDto
                        {
                            Id = ur.Role.Id,
                            NameEn = ur.Role.RoleNameEn,
                            NameFa = ur.Role.RoleNameFa
                        })));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
                .ForMember(dest => dest.UserSessions, opt => opt.Ignore())
                .ForMember(dest => dest.MenusAccesses, opt => opt.Ignore())
                .ForMember(dest => dest.Groups, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmailAddress, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.EmailAddressConfirmed, opt => opt.Ignore());
        }
    }
}