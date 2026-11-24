
using Asp.Versioning;
using SanjeshP.Payeh.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.WebFramework.Api;
using System.Text.Json;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>
    /// مدیریت کاربران.
    /// حذف به‌صورت نرم انجام می‌شود.
    /// همه خروجی‌ها در قالب ApiResult هستند.
    /// </summary>
    [ApiVersion("1")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IApiMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public UsersController(
            IUserService userService,
            IApiMapper mapper,
            IAuditLogService auditLogService)
        {
            _userService = userService;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// فهرست صفحه‌بندی‌شده کاربران.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResult<PagedResult<UserSelectDto>>),
            StatusCodes.Status200OK)]
        public async Task<ApiResult<PagedResult<UserSelectDto>>> Get(
            [FromQuery] UserQueryParameters query,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetPagedAsync(
                new UserQuery(
                    query.PageNumber,
                    query.PageSize,
                    query.Search,
                    query.IsActive,
                    query.RoleId,
                    query.SortBy,
                    query.SortDirection),
                cancellationToken);

            var items = result.Items.Select(_mapper.ToDto).ToList();

            return new PagedResult<UserSelectDto>(
                items,
                result.TotalCount,
                result.PageNumber,
                result.PageSize);
        }

        [HttpGet("{id:guid}")]
        public async Task<ApiResult<UserSelectDto>> GetById(
     [FromRoute] Guid id,
     CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);

            var profile = user.UserProfile?.FirstOrDefault();

            var result = new UserSelectDto
            {
                Id = user.Id,
                UserName = user.UserName,
                EmailAddress = user.EmailAddress,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                ExpireDate = user.ExpireDate,

                FirstName = profile?.FirstName,
                LastName = profile?.LastName,
                FatherName = profile?.FatherName,
                NationalCode = profile?.NationalCode,
                Gender = profile?.Gender,
                MaritalStatus = profile?.MaritalStatus,
                Religion = profile?.Religion,
                LeftHanded = profile?.LeftHanded,
                Nationality = profile?.Nationality,

                Roles = user.UserRoles?
                    .Where(x => x.Role != null)
                    .Select(x => new RoleDto
                    {
                        Id = x.Role.Id,
                        NameEn = x.Role.RoleNameEn,
                        NameFa = x.Role.RoleNameFa
                    })
                    .ToList() ?? new List<RoleDto>()
            };

            return result;
        }

        /// <summary>
        /// ایجاد کاربر جدید.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResult<UserSelectDto>),
            StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResult<UserSelectDto>>> Create(
            [FromBody] CreateUserDto dto,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.ToEntity(dto);

            var profile = new UserProfile
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                FatherName = dto.FatherName?.Trim(),
                NationalCode = dto.NationalCode.Trim(),
                Gender = dto.Gender,
                MaritalStatus = dto.MaritalStatus,
                Religion = dto.Religion,
                LeftHanded = dto.LeftHanded,
                Nationality = dto.Nationality
            };

            var created = await _userService.CreateAsync(
                entity,
                dto.Password,
                dto.RoleIds,
                profile,
                CurrentUserId,
                cancellationToken);

            var createdValues = new
            {
                created.UserName,
                EmailAddress = MaskEmail(created.EmailAddress),
                PhoneNumber = MaskPhone(created.PhoneNumber),
                created.IsActive,
                created.ExpireDate,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                RoleIds = created.UserRoles
                    .Select(x => x.RoleId)
                    .OrderBy(x => x)
                    .ToArray()
            };

            await _auditLogService.LogActivityAsync(
                action: "Users.Create",
                description: "ایجاد کاربر جدید",
                entityName: nameof(User),
                entityId: created.Id.ToString(),
                newValuesJson: JsonSerializer.Serialize(createdValues),
                cancellationToken: cancellationToken);

            var result = new ApiResult<UserSelectDto>(
                true,
                ApiResultStatusCode.Success,
                _mapper.ToDto(created),
                ApiMessages.Users.M001);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id, version = "1" },
                result);
        }

        /// <summary>
        /// ویرایش اطلاعات کاربر.
        /// فیلدهای null یا ارسال‌نشده تغییر نمی‌کنند.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResult<UserSelectDto>),
            StatusCodes.Status200OK)]
        public async Task<ApiResult<UserSelectDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateUserDto dto,
            CancellationToken cancellationToken)
        {
            var before = await _userService.GetByIdAsync(
                id,
                cancellationToken);

            var beforeProfile = before.UserProfile.FirstOrDefault();

            // اگر پروفایل وجود ندارد، برای ایجاد آن این سه فیلد ضروری‌اند.
            if (beforeProfile is null &&
                (string.IsNullOrWhiteSpace(dto.FirstName) ||
                 string.IsNullOrWhiteSpace(dto.LastName) ||
                 string.IsNullOrWhiteSpace(dto.NationalCode)))
            {
                throw new Common.Exceptions.BadRequestException(
                    "برای تکمیل پروفایل کاربر، نام، نام خانوادگی و کد ملی الزامی است.");
            }

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            // اطلاعات اصلی کاربر
            AddChange(
                oldValues, newValues,
                "UserName",
                before.UserName,
                dto.UserName,
                dto.UserName is not null &&
                !string.Equals(before.UserName, dto.UserName.Trim(),
                    StringComparison.Ordinal));

            AddChange(
                oldValues, newValues,
                "EmailAddress",
                MaskEmail(before.EmailAddress),
                MaskEmail(dto.EmailAddress),
                dto.EmailAddress is not null &&
                !string.Equals(before.EmailAddress, dto.EmailAddress.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            AddChange(
                oldValues, newValues,
                "PhoneNumber",
                MaskPhone(before.PhoneNumber),
                MaskPhone(dto.PhoneNumber),
                dto.PhoneNumber is not null &&
                !string.Equals(before.PhoneNumber, dto.PhoneNumber.Trim(),
                    StringComparison.Ordinal));

            AddChange(
                oldValues, newValues,
                "IsActive",
                before.IsActive,
                dto.IsActive,
                dto.IsActive.HasValue &&
                before.IsActive != dto.IsActive.Value);

            AddChange(
                oldValues, newValues,
                "ExpireDate",
                before.ExpireDate,
                dto.ExpireDate,
                dto.ExpireDate.HasValue &&
                before.ExpireDate != dto.ExpireDate.Value);

            // اطلاعات پروفایل
            if (beforeProfile is not null)
            {
                AddChange(oldValues, newValues, "FirstName",
                    beforeProfile.FirstName, dto.FirstName,
                    dto.FirstName is not null &&
                    beforeProfile.FirstName != dto.FirstName.Trim());

                AddChange(oldValues, newValues, "LastName",
                    beforeProfile.LastName, dto.LastName,
                    dto.LastName is not null &&
                    beforeProfile.LastName != dto.LastName.Trim());

                AddChange(oldValues, newValues, "FatherName",
                    beforeProfile.FatherName, dto.FatherName,
                    dto.FatherName is not null &&
                    beforeProfile.FatherName != dto.FatherName.Trim());

                // کد ملی در Audit ثبت نمی‌شود.
                if (dto.NationalCode is not null &&
                    beforeProfile.NationalCode != dto.NationalCode.Trim())
                {
                    oldValues["NationalCodeChanged"] = true;
                    newValues["NationalCodeChanged"] = true;
                }

                AddChange(oldValues, newValues, "Gender",
                    beforeProfile.Gender, dto.Gender,
                    dto.Gender.HasValue &&
                    beforeProfile.Gender != dto.Gender);

                AddChange(oldValues, newValues, "MaritalStatus",
                    beforeProfile.MaritalStatus, dto.MaritalStatus,
                    dto.MaritalStatus.HasValue &&
                    beforeProfile.MaritalStatus != dto.MaritalStatus);

                AddChange(oldValues, newValues, "Religion",
                    beforeProfile.Religion, dto.Religion,
                    dto.Religion.HasValue &&
                    beforeProfile.Religion != dto.Religion);

                AddChange(oldValues, newValues, "LeftHanded",
                    beforeProfile.LeftHanded, dto.LeftHanded,
                    dto.LeftHanded.HasValue &&
                    beforeProfile.LeftHanded != dto.LeftHanded);

                AddChange(oldValues, newValues, "Nationality",
                    beforeProfile.Nationality, dto.Nationality,
                    dto.Nationality.HasValue &&
                    beforeProfile.Nationality != dto.Nationality);
            }

            // نقش‌ها
            if (dto.RoleIds is not null)
            {
                var oldRoleIds = before.UserRoles
                    .Select(x => x.RoleId)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArray();

                var newRoleIds = dto.RoleIds
                    .Distinct()
                    .OrderBy(x => x)
                    .ToArray();

                if (!oldRoleIds.SequenceEqual(newRoleIds))
                {
                    oldValues["RoleIds"] = oldRoleIds;
                    newValues["RoleIds"] = newRoleIds;
                }
            }

            // اعمال تغییرات User
            var updated = await _userService.UpdateAsync(
                id,
                user =>
                {
                    if (dto.UserName is not null)
                        user.UserName = dto.UserName.Trim();

                    if (dto.EmailAddress is not null)
                    {
                        user.EmailAddress =
                            string.IsNullOrWhiteSpace(dto.EmailAddress)
                                ? null
                                : dto.EmailAddress.Trim();
                    }

                    if (dto.PhoneNumber is not null)
                    {
                        var newPhone = dto.PhoneNumber.Trim();

                        if (!string.Equals(
                            user.PhoneNumber,
                            newPhone,
                            StringComparison.Ordinal))
                        {
                            user.PhoneNumberConfirmed = false;
                        }

                        user.PhoneNumber = newPhone;
                    }

                    if (dto.IsActive.HasValue)
                        user.IsActive = dto.IsActive.Value;

                    if (dto.ExpireDate.HasValue)
                        user.ExpireDate = dto.ExpireDate.Value;
                },
                profile =>
                {
                    if (dto.FirstName is not null)
                        profile.FirstName = dto.FirstName.Trim();

                    if (dto.LastName is not null)
                        profile.LastName = dto.LastName.Trim();

                    if (dto.FatherName is not null)
                        profile.FatherName = dto.FatherName.Trim();

                    if (dto.NationalCode is not null)
                        profile.NationalCode = dto.NationalCode.Trim();

                    if (dto.Gender.HasValue)
                        profile.Gender = dto.Gender.Value;

                    if (dto.MaritalStatus.HasValue)
                        profile.MaritalStatus = dto.MaritalStatus.Value;

                    if (dto.Religion.HasValue)
                        profile.Religion = dto.Religion.Value;

                    if (dto.LeftHanded.HasValue)
                        profile.LeftHanded = dto.LeftHanded.Value;

                    if (dto.Nationality.HasValue)
                        profile.Nationality = dto.Nationality.Value;
                },
                dto.RoleIds,
                cancellationToken);

            // تغییر رمز پس از ذخیره اطلاعات
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                await _userService.SetPasswordAsync(
                    id,
                    dto.Password,
                    cancellationToken);

                await _auditLogService.LogActivityAsync(
                    action: "Users.PasswordChanged",
                    description: "تغییر رمز عبور کاربر توسط مدیر",
                    entityName: nameof(User),
                    entityId: id.ToString(),
                    eventType: "Security",
                    cancellationToken: cancellationToken);
            }

            // ثبت Audit تغییرات
            if (oldValues.Count > 0)
            {
                await _auditLogService.LogActivityAsync(
                    action: "Users.Update",
                    description: "ویرایش اطلاعات کاربر",
                    entityName: nameof(User),
                    entityId: id.ToString(),
                    oldValuesJson: JsonSerializer.Serialize(oldValues),
                    newValuesJson: JsonSerializer.Serialize(newValues),
                    cancellationToken: cancellationToken);
            }

            // اطلاعات تازه را مجدداً دریافت می‌کنیم تا خروجی شامل نقش‌ها و پروفایل باشد.
            var finalUser = await _userService.GetByIdAsync(
                id,
                cancellationToken);

            return _mapper.ToDto(finalUser);
        }

        /// <summary>
        /// حذف نرم کاربر و ابطال نشست‌های فعال.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ApiResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            if (id == CurrentUserId)
            {
                throw new Common.Exceptions.BadRequestException(
                    "حذف حساب کاربری خودتان مجاز نیست.");
            }

            var before = await _userService.GetByIdAsync(
                id,
                cancellationToken);

            await _userService.SoftDeleteAsync(
                id,
                cancellationToken);

            await _auditLogService.LogActivityAsync(
                action: "Users.Delete",
                description: "حذف نرم کاربر و غیرفعال‌سازی نشست‌ها",
                entityName: nameof(User),
                entityId: id.ToString(),
                oldValuesJson: JsonSerializer.Serialize(new
                {
                    before.IsActive,
                    IsDeleted = false
                }),
                newValuesJson: JsonSerializer.Serialize(new
                {
                    IsActive = false,
                    IsDeleted = true
                }),
                cancellationToken: cancellationToken);

            return new ApiResult(
                true,
                ApiResultStatusCode.Success,
                ApiMessages.Users.M002);
        }

        #region Audit Helpers

        private static void AddChange(
            IDictionary<string, object?> oldValues,
            IDictionary<string, object?> newValues,
            string propertyName,
            object? oldValue,
            object? newValue,
            bool hasChanged)
        {
            if (!hasChanged)
                return;

            oldValues[propertyName] = oldValue;
            newValues[propertyName] = newValue;
        }

        private static string? MaskPhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var phone = value.Trim();

            if (phone.Length <= 4)
                return "****";

            var visiblePrefix = phone[..Math.Min(3, phone.Length)];

            return $"{visiblePrefix}****{phone[^2..]}";
        }

        private static string? MaskEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var atIndex = value.IndexOf('@');

            if (atIndex <= 0)
                return "***";

            return $"{value[..1]}***{value[atIndex..]}";
        }

        #endregion
    }
}
