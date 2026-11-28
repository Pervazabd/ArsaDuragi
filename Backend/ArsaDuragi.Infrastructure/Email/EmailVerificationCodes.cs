using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace ArsaDuragi.Infrastructure.Email;

internal sealed class EmailVerificationCodes(IMemoryCache cache)
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ResendInterval = TimeSpan.FromMinutes(1);
    private const int MaximumFailedAttempts = 5;
    private readonly object _gate = new();

    public string? Create(Type purpose, string subject, string value)
    {
        var key = (purpose, subject);

        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;

            if (cache.TryGetValue<PendingCode>(key, out var current) &&
                current is not null && now < current.ResendAfter)
            {
                return null;
            }

            var code = RandomNumberGenerator.GetInt32(1_000_000)
                .ToString("D6", CultureInfo.InvariantCulture);

            cache.Set(
                key,
                new PendingCode(code, value, now + ResendInterval),
                now + Lifetime);

            return code;
        }
    }

    public string? Verify(Type purpose, string subject, string code)
    {
        var key = (purpose, subject);

        lock (_gate)
        {
            if (!cache.TryGetValue<PendingCode>(key, out var pending) ||
                pending is null || pending.FailedAttempts >= MaximumFailedAttempts)
            {
                return null;
            }

            if (string.Equals(pending.Code, code, StringComparison.Ordinal))
                return pending.Value;

            pending.FailedAttempts++;
            return null;
        }
    }

    private sealed class PendingCode(
        string code, string value, DateTimeOffset resendAfter)
    {
        public string Code { get; } = code;
        public string Value { get; } = value;
        public DateTimeOffset ResendAfter { get; } = resendAfter;
        public int FailedAttempts { get; set; }
    }
}