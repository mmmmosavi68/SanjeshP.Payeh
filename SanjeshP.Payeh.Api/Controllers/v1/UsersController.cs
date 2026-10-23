using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Entities.User;
using SanjeshP.Payeh.Services.Users;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.Controllers.V1
{
    /// <summary>
    /// مدیریت کاربران — فقط برای نقش‌های مجاز (Policy: UserManagement ⇒ نقش Admin).
    /// حذف به‌صورت نرم (Soft Delete) است و فهرست، صفحه‌بندی/جستجو/مرتب‌سازی/فیلتر دارد.
    /// همه‌ی خروجی‌ها در قالب ApiResult هستند.
    /// </summary>
    [ApiVersion("1")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>فهرست صفحه‌بندی‌شده‌ی کاربران با جستجو، فیلتر و مرتب‌سازی.</summary>
        /// <param name="query">پارامترهای کوئری (pageNumber، pageSize، search، isActive، roleId، sortBy، sortDirection).</param>
        /// <param name="cancellationToken">توکن لغو عملیات.</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<PagedResult<UserSelectDto>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<PagedResult<UserSelectDto>>> Get([FromQuery] UserQueryParameters query, CancellationToken cancellationToken)
        {
            var result = await _userService.GetPagedAsync(
                new UserQuery(query.PageNumber, query.PageSize, query.Search, query.IsActive, query.RoleId, query.SortBy, query.SortDirection),
                cancellationToken);
            var items = _mapper.Map<IReadOnlyList<UserSelectDto>>(result.Items);
            return new PagedResult<UserSelectDto>(items, result.TotalCount, result.PageNumber, result.PageSize);
        }

        /// <summary>جزئیات یک کاربر.</summary>
        /// <response code="404">کاربر یافت نشد.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResult<UserSelectDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<UserSelectDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            return _mapper.Map<UserSelectDto>(user);
        }

        /// <summary>افزودن کاربر جدید.</summary>
        /// <response code="201">کاربر ایجاد شد.</response>
        /// <response code="409">نام کاربری یا موبایل تکراری است.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult<UserSelectDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResult<UserSelectDto>>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<User>(dto);

            var created = await _userService.CreateAsync(
                entity, dto.Password, dto.RoleIds, dto.FirstName, dto.LastName, dto.NationalCode,
                createdBy: CurrentUserId, cancellationToken);

            var result = new ApiResult<UserSelectDto>(true, Common.ApiResultStatusCode.Success, _mapper.Map<UserSelectDto>(created), "کاربر با موفقیت ایجاد شد.");
            return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1" }, result);
        }

        /// <summary>ویرایش کاربر؛ فقط فیلدهای ارسالی اعمال می‌شوند.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResult<UserSelectDto>), StatusCodes.Status200OK)]
        public async Task<ApiResult<UserSelectDto>> Update([FromRoute] Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
        {
            var updated = await _userService.UpdateAsync(id, user =>
            {
                if (dto.EmailAddress is not null) user.EmailAddress = dto.EmailAddress;
                if (dto.PhoneNumber is not null)
                {
                    // تغییر شماره ⇒ نیاز به تأیید مجدد OTP
                    if (user.PhoneNumber != dto.PhoneNumber) user.PhoneNumberConfirmed = false;
                    user.PhoneNumber = dto.PhoneNumber;
                }
                if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            }, dto.RoleIds, dto.FirstName, dto.LastName, cancellationToken);

            return _mapper.Map<UserSelectDto>(updated);
        }

        /// <summary>حذف نرم کاربر (IsDeleted=true) و ابطال همه‌ی نشست‌های او.</summary>
        /// <response code="204">حذف انجام شد؛ بدنه‌ی پاسخ خالی است.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            // محافظت: کاربر نمی‌تواند خودش را حذف کند.
            if (id == CurrentUserId)
                throw new Common.Exceptions.BadRequestException("حذف حساب کاربری خودتان مجاز نیست.");

            await _userService.SoftDeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}