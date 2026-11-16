
namespace SanjeshP.Payeh.Common.Utilities;

public static class IranianNationalCodeValidator
{
    public static bool IsValid(string? nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode))
            return false;

        nationalCode = nationalCode.Fa2En().Trim();

        if (nationalCode.Length != 10 ||
            !nationalCode.All(char.IsAsciiDigit))
            return false;

        // کدهایی که تمام ارقام آن‌ها یکسان است معتبر نیستند.
        if (nationalCode.Distinct().Count() == 1)
            return false;

        var checkDigit = nationalCode[9] - '0';

        var sum = 0;

        for (var i = 0; i < 9; i++)
        {
            sum += (nationalCode[i] - '0') * (10 - i);
        }

        var remainder = sum % 11;

        var expectedCheckDigit = remainder < 2
            ? remainder
            : 11 - remainder;

        return checkDigit == expectedCheckDigit;
    }
}
