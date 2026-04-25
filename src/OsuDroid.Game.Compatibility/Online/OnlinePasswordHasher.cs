using System.Security.Cryptography;
using System.Text;

namespace OsuDroid.Game.Compatibility.Online;

public static class OnlinePasswordHasher
{
    private const string Salt = "taikotaiko";

    public static string HashPassword(string? password)
    {
        string escaped = EscapeHtmlSpecialCharacters(AddSlashes((password ?? string.Empty).Trim()));
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(escaped + Salt));
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
