
using System;
using System.Globalization;
using System.Text;

namespace SanjeshP.Payeh.Common.Utilities
{
    public static class StringExtensions
    {
        private static readonly CultureInfo PersianCulture =
            CultureInfo.GetCultureInfo("fa-IR");

        #region Validation

        public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
        {
            return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string NullIfEmpty(this string value)
        {
            return value != null && value.Length == 0 ? null : value;
        }

        public static string NullIfWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        #endregion

        #region Numeric Conversion

        public static int ToInt(this string value)
        {
            return int.Parse(NormalizeNumber(value), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static int ToInt(this string value, int defaultValue)
        {
            int result;
            return value.TryToInt(out result) ? result : defaultValue;
        }

        public static bool TryToInt(this string value, out int result)
        {
            return int.TryParse(NormalizeNumber(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static long ToLong(this string value)
        {
            return long.Parse(NormalizeNumber(value), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static bool TryToLong(this string value, out long result)
        {
            return long.TryParse(NormalizeNumber(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static decimal ToDecimal(this string value)
        {
            return decimal.Parse(NormalizeNumber(value), NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        public static decimal ToDecimal(this string value, decimal defaultValue)
        {
            decimal result;
            return value.TryToDecimal(out result) ? result : defaultValue;
        }

        public static bool TryToDecimal(this string value, out decimal result)
        {
            return decimal.TryParse(NormalizeNumber(value), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryToDouble(this string value, out double result)
        {
            return double.TryParse(NormalizeNumber(value), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryToBool(this string value, out bool result)
        {
            if (string.Equals(value?.Trim(), "بله", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }

            if (string.Equals(value?.Trim(), "خیر", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            return bool.TryParse(value, out result);
        }

        private static string NormalizeNumber(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string result = value
                .Fa2En()
                .Trim()
                .Replace(",", "")
                .Replace("٬", "")
                .Replace("٫", ".");

            return result;
        }

        #endregion

        #region Numeric Formatting

        public static string ToNumeric(this int value)
        {
            return value.ToString("N0");
        }

        public static string ToNumeric(this decimal value)
        {
            return value.ToString("N0");
        }

        public static string ToNumericFa(this int value)
        {
            return value.ToString("N0", PersianCulture).En2Fa();
        }

        public static string ToNumericFa(this decimal value)
        {
            return value.ToString("N0", PersianCulture).En2Fa();
        }

        public static string ToCurrency(this int value)
        {
            return value.ToString("C0");
        }

        public static string ToCurrency(this decimal value)
        {
            return value.ToString("C0");
        }

        public static string ToCurrencyFa(this decimal value, string unit = "ریال")
        {
            return value.ToString("N0", PersianCulture).En2Fa() + " " + unit;
        }

        public static string ToCurrencyFa(this int value, string unit = "ریال")
        {
            return ((decimal)value).ToCurrencyFa(unit);
        }

        #endregion

        #region Persian And Arabic Digits

        public static string En2Fa(this string value)
        {
            if (value == null)
                return null;

            StringBuilder result = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (c >= '0' && c <= '9')
                    result.Append((char)('\u06F0' + c - '0'));
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        public static string Fa2En(this string value)
        {
            if (value == null)
                return null;

            StringBuilder result = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (c >= '\u06F0' && c <= '\u06F9')
                    result.Append((char)('0' + c - '\u06F0'));
                else if (c >= '\u0660' && c <= '\u0669')
                    result.Append((char)('0' + c - '\u0660'));
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        #endregion

        #region Persian Text Normalization

        public static string FixPersianChars(this string value)
        {
            if (value == null)
                return null;

            StringBuilder result = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                switch (c)
                {
                    // Arabic kaf and Persian kaf variants
                    case '\u0643':
                    case '\u06AA':
                    case '\u06AB':
                    case '\u06AE':
                    case '\uFB8E':
                    case '\uFB8F':
                    case '\uFEDA':
                    case '\uFEDF':
                    case '\uFEE0':
                    case '\uFEE1':
                        result.Append('ک');
                        break;

                    // Arabic yeh and Persian yeh variants
                    case '\u064A':
                    case '\u0649':
                    case '\u06CC':
                    case '\u06CD':
                    case '\u06CE':
                    case '\uFEEF':
                    case '\uFEF0':
                    case '\uFEF1':
                    case '\uFEF2':
                        result.Append('ی');
                        break;

                    // Arabic heh variants
                    case '\u0647':
                    case '\u06D5':
                    case '\u06BE':
                    case '\u06C1':
                        result.Append('ه');
                        break;

                    // Non-breaking space
                    case '\u00A0':
                    case '\u202F':
                        result.Append(' ');
                        break;

                    // Zero-width non-joiner: preserve Persian half-space
                    case '\u200C':
                        result.Append('\u200C');
                        break;

                    default:
                        result.Append(c);
                        break;
                }
            }

            return result.ToString();
        }

        public static string NormalizeSpaces(this string value)
        {
            if (value == null)
                return null;

            StringBuilder result = new StringBuilder(value.Length);
            bool previousWasSpace = false;

            foreach (char c in value.Trim())
            {
                if (char.IsWhiteSpace(c) || c == '\u00A0')
                {
                    if (!previousWasSpace)
                        result.Append(' ');

                    previousWasSpace = true;
                }
                else
                {
                    result.Append(c);
                    previousWasSpace = false;
                }
            }

            return result.ToString();
        }

        public static string CleanString(this string value)
        {
            if (value == null)
                return null;

            return value.FixPersianChars().Fa2En().NormalizeSpaces().NullIfEmpty();
        }

        public static string CleanPersianString(this string value)
        {
            if (value == null)
                return null;

            return value.FixPersianChars().NormalizeSpaces().NullIfEmpty();
        }

        #endregion

        #region String Helpers

        public static string DefaultIfEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            if (value == null || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength);
        }

        public static string Left(this string value, int length)
        {
            return value.Truncate(length);
        }

        public static string Right(this string value, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (value == null || value.Length <= length)
                return value;

            return value.Substring(value.Length - length);
        }

        public static bool ContainsIgnoreCase(this string value, string searchValue)
        {
            return value != null
                && searchValue != null
                && value.IndexOf(
                    searchValue,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
