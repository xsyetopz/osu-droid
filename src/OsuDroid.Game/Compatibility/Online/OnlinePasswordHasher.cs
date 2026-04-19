using System.Security.Cryptography;
using System.Text;

namespace OsuDroid.Game.Compatibility.Online;

public static class OnlinePasswordHasher
{
    private const string salt = "taikotaiko";

    public static string HashPassword(string? password)
    {
        var escaped = EscapeHtmlSpecialCharacters(AddSlashes((password ?? string.Empty).Trim()));
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(escaped + salt));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string AddSlashes(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("'", "\\'", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal)
        .Replace("\0", "\\0", StringComparison.Ordinal);

    public static string EscapeHtmlSpecialCharacters(string value) => value
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("\"", "&quot;", StringComparison.Ordinal)
        .Replace("'", "&#039;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);
}
