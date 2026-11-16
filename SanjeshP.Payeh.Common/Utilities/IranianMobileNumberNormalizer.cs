
namespace SanjeshP.Payeh.Common.Utilities;

public static class IranianMobileNumberNormalizer
{
    public static bool TryNormalize(
        string? phoneNumber,
        out string normalizedNumber)
    {
        normalizedNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var value = phoneNumber.Fa2En().Trim();

        // حذف جداکننده‌های رایج
        value = value
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        // پذیرش پیش‌شماره بین‌المللی فقط در قالب +98
        if (value.StartsWith("+"))
        {
            if (!value.StartsWith("+98"))
                return false;

            value = value[3..];
        }
        else if (value.StartsWith("0098"))
        {
            value = value[4..];
        }
        else if (value.StartsWith("98") && value.Length == 12)
        {
            value = value[2..];
        }

        // تبدیل شماره بدون صفر ابتدایی
        if (value.Length == 10 && value.StartsWith("9"))
            value = "0" + value;

        // کنترل ساختار شماره همراه ایران
        if (value.Length != 11 ||
            !value.StartsWith("09") ||
            !value.All(char.IsAsciiDigit))
        {
            return false;
        }

        normalizedNumber = value;
        return true;
    }
}
