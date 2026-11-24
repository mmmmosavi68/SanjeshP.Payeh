using Microsoft.AspNetCore.Http;
using SanjeshP.Payeh.Common;

namespace SanjeshP.Payeh.WebFramework.Api;

public static class PagingResponseExtensions
{
    /// <summary>هدرهای صفحه‌بندی (افزودنی؛ بدنه‌ی JSON بدون تغییر می‌ماند).</summary>
    public static void ApplyPaging<T>(this HttpResponse response, PagedItems<T> paged)
    {
        if (!paged.IsPaged)
            return;

        var totalPages = paged.PageSize == 0 ? 0 : (int)Math.Ceiling(paged.TotalCount / (double)paged.PageSize);
        response.Headers["X-Total-Count"] = paged.TotalCount.ToString();
        response.Headers["X-Page"] = paged.Page.ToString();
        response.Headers["X-Page-Size"] = paged.PageSize.ToString();
        response.Headers["X-Total-Pages"] = totalPages.ToString();
        response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count,X-Page,X-Page-Size,X-Total-Pages";
    }
}
