using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Services.Captcha;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace SanjeshP.Payeh.Services.Captcha
{
    /// <summary>
    /// کپچای تصویری محلی. پاسخ خام فقط هنگام تولید تصویر در حافظه‌ی جاری وجود دارد؛
    /// در Cache فقط هش پاسخ، IP و تعداد تلاش‌ها نگهداری می‌شود.
    /// </summary>
    public sealed class CaptchaService : ICaptchaService, ICaptchaImageService
    {
        private const string CachePrefix = "Payeh:Captcha:";
        private readonly IMemoryCache _cache;
        private readonly CaptchaSettings _settings;

        private static readonly string[][] DigitFont =
        [
            ["01110", "10001", "10011", "10101", "11001", "10001", "01110"], // 0
            ["00100", "01100", "00100", "00100", "00100", "00100", "01110"], // 1
            ["01110", "10001", "00001", "00010", "00100", "01000", "11111"], // 2
            ["11110", "00001", "00001", "01110", "00001", "00001", "11110"], // 3
            ["00010", "00110", "01010", "10010", "11111", "00010", "00010"], // 4
            ["11111", "10000", "10000", "11110", "00001", "00001", "11110"], // 5
            ["01110", "10000", "10000", "11110", "10001", "10001", "01110"], // 6
            ["11111", "00001", "00010", "00100", "01000", "01000", "01000"], // 7
            ["01110", "10001", "10001", "01110", "10001", "10001", "01110"], // 8
            ["01110", "10001", "10001", "01111", "00001", "00001", "01110"]  // 9
        ];

        public CaptchaService(IMemoryCache cache, IOptions<SiteSettings> siteSettings)
        {
            _cache = cache;
            _settings = siteSettings.Value.CaptchaSettings;
        }

        public bool IsEnabled => _settings.Enabled;

        public Task<CaptchaChallenge> CreateAsync(string? remoteIp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var length = Math.Clamp(_settings.CodeLength, 4, 8);
            var expires = Math.Clamp(_settings.ExpirationSeconds, 30, 600);
            var maxAttempts = Math.Clamp(_settings.MaxAttempts, 1, 10);
            var width = Math.Clamp(_settings.Width, 160, 320);
            var height = Math.Clamp(_settings.Height, 56, 100);

            var upperBound = (int)Math.Pow(10, length);
            var answer = RandomNumberGenerator.GetInt32(0, upperBound).ToString($"D{length}");
            var captchaId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var answerHash = HashAnswer(captchaId, answer);

            var entry = new CaptchaEntry(answerHash, remoteIp ?? string.Empty, maxAttempts);
            _cache.Set(CachePrefix + captchaId, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expires),
                Size = 1
            });

            var png = PngCaptchaRenderer.Render(answer, width, height);
            var result = new CaptchaChallenge(
                captchaId,
                Convert.ToBase64String(png),
                "image/png",
                expires);

            return Task.FromResult(result);
        }

        public Task<bool> ValidateAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult(false);

            var separator = token.IndexOf('.');
            if (separator != 64 || separator == token.Length - 1)
                return Task.FromResult(false);

            var captchaId = token[..separator];
            var answer = NormalizeDigits(token[(separator + 1)..].Trim());
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > 8 || answer.Any(c => c is < '0' or > '9'))
                return Task.FromResult(false);

            var key = CachePrefix + captchaId;
            if (!_cache.TryGetValue(key, out CaptchaEntry? entry) || entry is null)
                return Task.FromResult(false);

            lock (entry.SyncRoot)
            {
                if (entry.Consumed || entry.Attempts >= entry.MaxAttempts)
                {
                    _cache.Remove(key);
                    return Task.FromResult(false);
                }

                if (!string.Equals(entry.RemoteIp, remoteIp ?? string.Empty, StringComparison.Ordinal))
                {
                    entry.Attempts++;
                    if (entry.Attempts >= entry.MaxAttempts)
                        _cache.Remove(key);
                    return Task.FromResult(false);
                }

                var suppliedHash = HashAnswer(captchaId, answer);
                var valid = CryptographicOperations.FixedTimeEquals(entry.AnswerHash, suppliedHash);
                if (!valid)
                {
                    entry.Attempts++;
                    if (entry.Attempts >= entry.MaxAttempts)
                        _cache.Remove(key);
                    return Task.FromResult(false);
                }

                entry.Consumed = true;
                _cache.Remove(key);
                return Task.FromResult(true);
            }
        }

        private static byte[] HashAnswer(string captchaId, string answer)
            => SHA256.HashData(Encoding.UTF8.GetBytes(captchaId + ":" + answer));

        private static string NormalizeDigits(string value)
        {
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] is >= '\u06F0' and <= '\u06F9')
                    chars[i] = (char)('0' + chars[i] - '\u06F0');
                else if (chars[i] is >= '\u0660' and <= '\u0669')
                    chars[i] = (char)('0' + chars[i] - '\u0660');
            }
            return new string(chars);
        }

        private sealed class CaptchaEntry(byte[] answerHash, string remoteIp, int maxAttempts)
        {
            public object SyncRoot { get; } = new();
            public byte[] AnswerHash { get; } = answerHash;
            public string RemoteIp { get; } = remoteIp;
            public int MaxAttempts { get; } = maxAttempts;
            public int Attempts { get; set; }
            public bool Consumed { get; set; }
        }

        /// <summary>تولید PNG بدون System.Drawing و بدون وابستگی به پکیج خارجی.</summary>
        private static class PngCaptchaRenderer
        {
            public static byte[] Render(string text, int width, int height)
            {
                var pixels = new byte[height, width, 3];
                FillBackground(pixels, width, height);

                for (var i = 0; i < width * height / 18; i++)
                {
                    var x = RandomNumberGenerator.GetInt32(width);
                    var y = RandomNumberGenerator.GetInt32(height);
                    SetPixel(pixels, x, y, 170, 180, 190);
                }

                for (var i = 0; i < 5; i++)
                {
                    var y1 = RandomNumberGenerator.GetInt32(height);
                    var y2 = RandomNumberGenerator.GetInt32(height);
                    DrawLine(pixels, 0, y1, width - 1, y2, 150, 165, 180);
                }

                const int scale = 5;
                const int glyphWidth = 5;
                const int gap = 5;
                var totalWidth = text.Length * glyphWidth * scale + (text.Length - 1) * gap;
                var startX = Math.Max(4, (width - totalWidth) / 2);
                var startY = (height - 7 * scale) / 2;

                for (var index = 0; index < text.Length; index++)
                {
                    var digit = text[index] - '0';
                    var offsetX = startX + index * (glyphWidth * scale + gap);
                    var color = (R: (byte)RandomNumberGenerator.GetInt32(25, 75),
                                 G: (byte)RandomNumberGenerator.GetInt32(45, 95),
                                 B: (byte)RandomNumberGenerator.GetInt32(80, 140));

                    for (var row = 0; row < 7; row++)
                    for (var col = 0; col < 5; col++)
                    {
                        if (DigitFont[digit][row][col] != '1')
                            continue;

                        for (var sy = 0; sy < scale; sy++)
                        for (var sx = 0; sx < scale; sx++)
                            SetPixel(pixels, offsetX + col * scale + sx,
                                startY + row * scale + sy, color.R, color.G, color.B);
                    }
                }

                return EncodePng(pixels, width, height);
            }

            private static void FillBackground(byte[,,] pixels, int width, int height)
            {
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    pixels[y, x, 0] = 247;
                    pixels[y, x, 1] = 249;
                    pixels[y, x, 2] = 252;
                }
            }

            private static void SetPixel(byte[,,] pixels, int x, int y, byte r, byte g, byte b)
            {
                if (x < 0 || y < 0 || y >= pixels.GetLength(0) || x >= pixels.GetLength(1))
                    return;
                pixels[y, x, 0] = r;
                pixels[y, x, 1] = g;
                pixels[y, x, 2] = b;
            }

            private static void DrawLine(byte[,,] pixels, int x0, int y0, int x1, int y1, byte r, byte g, byte b)
            {
                var dx = Math.Abs(x1 - x0);
                var sx = x0 < x1 ? 1 : -1;
                var dy = -Math.Abs(y1 - y0);
                var sy = y0 < y1 ? 1 : -1;
                var error = dx + dy;
                while (true)
                {
                    SetPixel(pixels, x0, y0, r, g, b);
                    if (x0 == x1 && y0 == y1) break;
                    var twice = 2 * error;
                    if (twice >= dy) { error += dy; x0 += sx; }
                    if (twice <= dx) { error += dx; y0 += sy; }
                }
            }

            private static byte[] EncodePng(byte[,,] pixels, int width, int height)
            {
                using var output = new MemoryStream();
                output.Write([137, 80, 78, 71, 13, 10, 26, 10]);

                var ihdr = new byte[13];
                WriteUInt32(ihdr, 0, (uint)width);
                WriteUInt32(ihdr, 4, (uint)height);
                ihdr[8] = 8; // bit depth
                ihdr[9] = 2; // truecolor RGB
                WriteChunk(output, "IHDR", ihdr);

                using var raw = new MemoryStream();
                for (var y = 0; y < height; y++)
                {
                    raw.WriteByte(0); // PNG filter: None
                    for (var x = 0; x < width; x++)
                    {
                        raw.WriteByte(pixels[y, x, 0]);
                        raw.WriteByte(pixels[y, x, 1]);
                        raw.WriteByte(pixels[y, x, 2]);
                    }
                }

                using var compressed = new MemoryStream();
                using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
                    zlib.Write(raw.ToArray());

                WriteChunk(output, "IDAT", compressed.ToArray());
                WriteChunk(output, "IEND", []);
                return output.ToArray();
            }

            private static void WriteChunk(Stream stream, string type, byte[] data)
            {
                Span<byte> length = stackalloc byte[4];
                WriteUInt32(length, 0, (uint)data.Length);
                stream.Write(length);

                var typeBytes = Encoding.ASCII.GetBytes(type);
                stream.Write(typeBytes);
                stream.Write(data);

                var crcInput = new byte[typeBytes.Length + data.Length];
                Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
                Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
                Span<byte> crc = stackalloc byte[4];
                WriteUInt32(crc, 0, Crc32(crcInput));
                stream.Write(crc);
            }

            private static uint Crc32(byte[] data)
            {
                var crc = 0xFFFFFFFFu;
                foreach (var value in data)
                {
                    crc ^= value;
                    for (var bit = 0; bit < 8; bit++)
                        crc = (crc & 1) != 0 ? 0xEDB88320u ^ (crc >> 1) : crc >> 1;
                }
                return ~crc;
            }

            private static void WriteUInt32(Span<byte> target, int offset, uint value)
            {
                target[offset] = (byte)(value >> 24);
                target[offset + 1] = (byte)(value >> 16);
                target[offset + 2] = (byte)(value >> 8);
                target[offset + 3] = (byte)value;
            }
        }
    }
}
