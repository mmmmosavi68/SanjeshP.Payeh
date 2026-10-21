using Asp.Versioning;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Common;

// اصلاح Namespace: SanjeshPWebFramework.Api → SanjeshP.Payeh.WebFramework.Api
namespace SanjeshP.Payeh.WebFramework.Api
{
    /// <summary>
    /// کنترلر CRUD ژنریک برای موجودیت‌های ساده.
    /// اصلاحات نسبت به نسخه‌ی قبل:
    /// - همه‌ی اکشن‌ها ApiResult برمی‌گردانند (Get لیست هم همین‌طور).
    /// - مسیر Update به [HttpPut("{id}")] اصلاح شد (قبلاً id در مسیر نبود).
    /// - فیلد بی‌استفاده‌ی repository حذف شد.
    /// - در Update/Delete نبودِ رکورد ⇒ NotFound به‌جای NullReferenceException.
    /// </summary>
    [ApiVersion("1")]
    public class CrudController<TDto, TSelectDto, TEntity, TKey> : BaseController
        where TDto : BaseDto<TDto, TEntity, TKey>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()
        where TEntity : class, IEntity<TKey>, new()
    {
        protected readonly IEFRepository<TEntity> Repository;
        protected readonly IMapper Mapper;

        public CrudController(IEFRepository<TEntity> repository, IMapper mapper)
        {
            Repository = repository;
            Mapper = mapper;
        }

        /// <summary>فهرست کامل (برای منابع کوچک؛ منابع بزرگ باید صفحه‌بندی اختصاصی بگیرند).</summary>
        [HttpGet]
        public virtual async Task<ApiResult<List<TSelectDto>>> Get(CancellationToken cancellationToken)
        {
            var list = await Repository.TableNoTracking
                .ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return list;
        }

        /// <summary>جزئیات یک رکورد بر اساس شناسه.</summary>
        [HttpGet("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Get(TKey id, CancellationToken cancellationToken)
        {
            var dto = await Repository.TableNoTracking
                .ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id!.Equals(id), cancellationToken);

            if (dto is null)
                return NotFound();

            return dto;
        }

        /// <summary>ایجاد رکورد جدید (HTTP 201).</summary>
        [HttpPost]
        public virtual async Task<ActionResult<ApiResult<TSelectDto>>> Create(TDto dto, CancellationToken cancellationToken)
        {
            var model = dto.ToEntity(Mapper);

            await Repository.AddAsync(model, cancellationToken);

            var resultDto = await Repository.TableNoTracking
                .ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
                .SingleAsync(p => p.Id!.Equals(model.Id), cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = model.Id, version = "1" }, new ApiResult<TSelectDto>(true, Common.ApiResultStatusCode.Success, resultDto));
        }

        /// <summary>ویرایش رکورد موجود.</summary>
        [HttpPut("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Update(TKey id, TDto dto, CancellationToken cancellationToken)
        {
            var model = await Repository.GetByIdAsync(cancellationToken, id!);
            if (model is null)
                return NotFound();

            model = dto.ToEntity(Mapper, model);

            await Repository.UpdateAsync(model, cancellationToken);

            var resultDto = await Repository.TableNoTracking
                .ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
                .SingleAsync(p => p.Id!.Equals(model.Id), cancellationToken);

            return resultDto!;
        }

        /// <summary>حذف رکورد (فیزیکی؛ برای موجودیت‌های دارای IsDeleted از Soft Delete در سرویس استفاده کنید).</summary>
        [HttpDelete("{id}")]
        public virtual async Task<ApiResult> Delete(TKey id, CancellationToken cancellationToken)
        {
            var model = await Repository.GetByIdAsync(cancellationToken, id!);
            if (model is null)
                return NotFound();

            await Repository.DeleteAsync(model, cancellationToken);
            return Ok();
        }
    }

    /// <summary>نسخه‌ی کوتاه با کلید int.</summary>
    public class CrudController<TDto, TSelectDto, TEntity> : CrudController<TDto, TSelectDto, TEntity, int>
        where TDto : BaseDto<TDto, TEntity, int>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, int>, new()
        where TEntity : class, IEntity<int>, new()
    {
        public CrudController(IEFRepository<TEntity> repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }

    /// <summary>نسخه‌ی کوتاه با DTO مشترک برای ورودی و خروجی.</summary>
    public class CrudController<TDto, TEntity> : CrudController<TDto, TDto, TEntity, int>
        where TDto : BaseDto<TDto, TEntity, int>, new()
        where TEntity : class, IEntity<int>, new()
    {
        public CrudController(IEFRepository<TEntity> repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }
}