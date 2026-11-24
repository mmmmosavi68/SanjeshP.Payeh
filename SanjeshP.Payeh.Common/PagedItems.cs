namespace SanjeshP.Payeh.Common;

/// <summary>نتیجه‌ی صفحه‌بندی اختیاری؛ بدون page/pageSize همه‌ی رکوردها برگردانده می‌شود (IsPaged=false).</summary>
public sealed record PagedItems<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, bool IsPaged);
