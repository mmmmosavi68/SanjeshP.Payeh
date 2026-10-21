namespace SanjeshP.Payeh.WebFramework.Api
{
    /// <summary>
    /// ساختار استاندارد داده‌ی صفحه‌بندی‌شده.
    /// همیشه داخل ApiResult برمی‌گردد: ApiResult&lt;PagedResult&lt;TDto&gt;&gt;
    /// تا فرمت کلی پاسخ‌ها یکدست بماند.
    /// </summary>
    /// <typeparam name="T">نوع آیتم‌های صفحه (DTO).</typeparam>
    public class PagedResult<T>
    {
        /// <summary>آیتم‌های صفحه‌ی جاری.</summary>
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

        /// <summary>کل تعداد رکوردهای منطبق با فیلتر (نه فقط این صفحه).</summary>
        public int TotalCount { get; set; }

        /// <summary>شماره‌ی صفحه‌ی جاری (از 1).</summary>
        public int PageNumber { get; set; }

        /// <summary>اندازه‌ی صفحه.</summary>
        public int PageSize { get; set; }

        /// <summary>تعداد کل صفحه‌ها.</summary>
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>آیا صفحه‌ی قبلی وجود دارد؟</summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>آیا صفحه‌ی بعدی وجود دارد؟</summary>
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult() { }

        public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}