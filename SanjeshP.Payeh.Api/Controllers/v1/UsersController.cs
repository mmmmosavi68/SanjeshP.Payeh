
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Services.Users;
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
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IAuditLogService auditLogService)
        {
            _userService = userService;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// فهرست صفحه‌بندی‌شده کاربران با جستجو، فیلتر و مرتب‌سازی.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResult<PagedResult<UserSelectDto>>),
            StatusCodes.Status200OK)]
        public async Task<ApiResult<PagedResult<UserSelectDto>>> Get([FromQuery] UserQueryParameters query, CancellationToken cancellationToken)
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

            var items = _mapper.Map<IReadOnlyList<UserSelectDto>>(result.Items);

            return new PagedResult<UserSelectDto>(
                items,
                result.TotalCount,
                result.PageNumber,
                result.PageSize);
        }

        /// <summary>
        /// دریافت جزئیات یک کاربر.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResult<UserSelectDto>),
            StatusCodes.Status200OK)]
        public async Task<ApiResult<UserSelectDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);

            return _mapper.Map<UserSelectDto>(user);
        }

        /// <summary>
        /// ایجاد کاربر جدید.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResult<UserSelectDto>),
            StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResult<UserSelectDto>>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<User>(dto);

            var created = await _userService.CreateAsync(
                entity,
                dto.Password,
                dto.RoleIds,
                dto.FirstName,
                dto.LastName,
                dto.NationalCode,
                createdBy: CurrentUserId,
                cancellationToken);

            // اطلاعات حساس مانند رمز عبور و کد ملی وارد Audit نمی‌شوند.
            // ایمیل و شماره موبایل نیز به‌صورت پوشیده ثبت می‌شوند.
            var createdValues = new
            {
                created.UserName,
                EmailAddress = MaskEmail(created.EmailAddress),
                PhoneNumber = MaskPhone(created.PhoneNumber),
                created.IsActive,

                FirstName = created.UserProfile
                    .FirstOrDefault()?.FirstName,

                LastName = created.UserProfile
                    .FirstOrDefault()?.LastName,

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
                Common.ApiResultStatusCode.Success,
                _mapper.Map<UserSelectDto>(created),
                "کاربر با موفقیت ایجاد شد.");

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id, version = "1" },
                result);
        }

        /// <summary>
        /// ویرایش اطلاعات کاربر.
        /// فقط فیلدهای ارسال‌شده اعمال می‌شوند.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResult<UserSelectDto>),
            StatusCodes.Status200OK)]
        public async Task<ApiResult<UserSelectDto>> Update([FromRoute] Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
        {
            // دریافت اطلاعات قبل از تغییر برای ثبت مقادیر قبلی.
            var before = await _userService.GetByIdAsync(
                id,
                cancellationToken);

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            #region EmailAddress

            AddChange(
                oldValues,
                newValues,
                "EmailAddress",
                MaskEmail(before.EmailAddress),
                MaskEmail(dto.EmailAddress),
                dto.EmailAddress is not null &&
                !string.Equals(
                    before.EmailAddress,
                    dto.EmailAddress,
                    StringComparison.Ordinal));

            #endregion

            #region PhoneNumber

            AddChange(
                oldValues,
                newValues,
                "PhoneNumber",
                MaskPhone(before.PhoneNumber),
                MaskPhone(dto.PhoneNumber),
                dto.PhoneNumber is not null &&
                !string.Equals(
                    before.PhoneNumber,
                    dto.PhoneNumber,
                    StringComparison.Ordinal));

            // تغییر شماره موبایل باعث نیاز به تأیید مجدد می‌شود.
            if (dto.PhoneNumber is not null &&
                !string.Equals(
                    before.PhoneNumber,
                    dto.PhoneNumber,
                    StringComparison.Ordinal))
            {
                oldValues["PhoneNumberConfirmed"] =
                    before.PhoneNumberConfirmed;

                newValues["PhoneNumberConfirmed"] = false;
            }

            #endregion

            #region IsActive

            AddChange(
                oldValues,
                newValues,
                "IsActive",
                before.IsActive,
                dto.IsActive,
                dto.IsActive.HasValue &&
                before.IsActive != dto.IsActive.Value);

            #endregion

            #region UserProfile

            // نام و نام خانوادگی در موجودیت UserProfile نگهداری می‌شوند.
            // سرویس فعلی فقط پروفایل موجود را به‌روزرسانی می‌کند.
            var beforeProfile = before.UserProfile.FirstOrDefault();

            if (beforeProfile is not null)
            {
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                {
                    var newFirstName = dto.FirstName.Trim();

                    AddChange(
                        oldValues,
                        newValues,
                        "FirstName",
                        beforeProfile.FirstName,
                        newFirstName,
                        !string.Equals(
                            beforeProfile.FirstName,
                            newFirstName,
                            StringComparison.Ordinal));
                }

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                {
                    var newLastName = dto.LastName.Trim();

                    AddChange(
                        oldValues,
                        newValues,
                        "LastName",
                        beforeProfile.LastName,
                        newLastName,
                        !string.Equals(
                            beforeProfile.LastName,
                            newLastName,
                            StringComparison.Ordinal));
                }
            }

            #endregion

            #region UserRoles

            // نقش‌ها از طریق رابطه UserRoles در موجودیت User قابل دسترسی هستند.
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

            #endregion

            // اعمال تغییرات اطلاعات اصلی کاربر، پروفایل و نقش‌ها.
            var updated = await _userService.UpdateAsync(
                id,
                user =>
                {
                    if (dto.EmailAddress is not null)
                    {
                        user.EmailAddress = dto.EmailAddress;
                    }

                    if (dto.PhoneNumber is not null)
                    {
                        if (!string.Equals(
                                user.PhoneNumber,
                                dto.PhoneNumber,
                                StringComparison.Ordinal))
                        {
                            user.PhoneNumberConfirmed = false;
                        }

                        user.PhoneNumber = dto.PhoneNumber;
                    }

                    if (dto.IsActive.HasValue)
                    {
                        user.IsActive = dto.IsActive.Value;
                    }
                },
                dto.RoleIds,
                dto.FirstName,
                dto.LastName,
                cancellationToken);

            // ثبت تغییرات اطلاعات کاربر پس از ذخیره موفق.
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

            // تغییر رمز عبور به‌صورت رویداد امنیتی مستقل ثبت می‌شود.
            // هیچ‌گاه رمز قبلی یا جدید در Audit ذخیره نمی‌شود.
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

            return _mapper.Map<UserSelectDto>(updated);
        }

        /// <summary>
        /// حذف نرم کاربر و ابطال نشست‌های فعال او.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,     CancellationToken cancellationToken)
        {
            // کاربر اجازه حذف حساب خودش را ندارد.
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

            return NoContent();
        }

        #region Audit Helpers

        /// <summary>
        /// در صورت تغییر مقدار، مقدار قبلی و جدید را به دیکشنری‌ها اضافه می‌کند.
        /// </summary>
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

        /// <summary>
        /// پوشاندن شماره موبایل برای جلوگیری از ذخیره اطلاعات کامل تماس در Audit.
        /// </summary>
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

        /// <summary>
        /// پوشاندن بخش محلی ایمیل.
        /// </summary>
        private static string? MaskEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var atIndex = value.IndexOf('@');

            if (atIndex <= 0)
                return "***";

            var localPart = value[..atIndex];
            var visiblePart = localPart[..1];

            return $"{visiblePart}***{value[atIndex..]}";
        }

        #endregion
    }
}
