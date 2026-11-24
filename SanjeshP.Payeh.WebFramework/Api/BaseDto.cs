using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.WebFramework.Api
{
    /// <summary>پایه‌ی DTOها؛ نگاشت‌ها در IApiMapper (Mapperly) و زمان Build اعتبارسنجی می‌شوند.</summary>
    public abstract class BaseDto<TDto, TEntity, TKey>
        where TDto : class, new()
        where TEntity : class, IEntity<TKey>, new()
    {
        [Display(Name = "ردیف")]
        public TKey Id { get; set; } = default!;
    }

    public abstract class BaseDto<TDto, TEntity> : BaseDto<TDto, TEntity, int>
        where TDto : class, new()
        where TEntity : class, IEntity<int>, new()
    {
    }
}
