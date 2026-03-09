using System.Text.RegularExpressions;

namespace EcoTrack.WebApi.Configuration;

public static class ConnectionStringNormalizer
{
    public static string Normalize(string value)
    {
        var normalized = value.Trim().Trim('"').Trim('\'');

        normalized = Regex.Replace(normalized, @"([?&])channel_binding=[^&]*", string.Empty, RegexOptions.IgnoreCase);
        normalized = Regex.Replace(normalized, @"(?:^|;)\s*channel_binding\s*=\s*[^;]*", string.Empty, RegexOptions.IgnoreCase).TrimEnd(';');

        if (normalized.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = Regex.Replace(normalized, @"([?&])sslmode(?=(&|$))", "$1sslmode=require", RegexOptions.IgnoreCase);

            if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            {
                var userInfo = uri.UserInfo?.Split(':', 2, StringSplitOptions.None) ?? Array.Empty<string>();
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var database = uri.AbsolutePath.Trim('/');
                var port = uri.IsDefaultPort ? 5432 : uri.Port;
                var sslMode = IsLocalHost(uri.Host) ? "Disable" : "Require";

                if (!string.IsNullOrWhiteSpace(uri.Host) && !string.IsNullOrWhiteSpace(database))
                {
                    return sslMode == "Disable"
                        ? $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Disable"
                        : $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
                }
            }
        }

        var hostMatch = Regex.Match(normalized, @"(?:^|;)\s*Host\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
        var host = hostMatch.Success ? hostMatch.Groups[1].Value.Trim() : string.Empty;
        var local = IsLocalHost(host);

        normalized = Regex.Replace(normalized, @"(?:^|;)\s*SSL\s*Mode\s*=\s*[^;]*", string.Empty, RegexOptions.IgnoreCase).TrimEnd(';');
        normalized = Regex.Replace(normalized, @"([?&])sslmode=[^&]*", string.Empty, RegexOptions.IgnoreCase);

        if (local)
            return normalized + ";SSL Mode=Disable";

        return normalized + ";SSL Mode=Require;Trust Server Certificate=true";
    }

    public static bool IsLocalHost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }
}
