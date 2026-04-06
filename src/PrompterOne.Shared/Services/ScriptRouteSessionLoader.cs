using PrompterOne.Core.Abstractions;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Services;

internal static class ScriptRouteSessionLoader
{
    public static string ResolveRequestedScriptId(string? scriptId, string currentUri)
    {
        if (!string.IsNullOrWhiteSpace(scriptId))
        {
            return scriptId.Trim();
        }

        return ResolveQueryValue(currentUri, AppRoutes.ScriptIdQueryKey);
    }

    public static async Task<bool> EnsureRequestedSessionAsync(
        string? scriptId,
        string currentUri,
        IScriptRepository scriptRepository,
        IScriptSessionService sessionService)
    {
        var resolvedScriptId = ResolveRequestedScriptId(scriptId, currentUri);
        return await EnsureRequestedSessionAsync(
            resolvedScriptId,
            scriptRepository,
            sessionService);
    }

    public static async Task<bool> EnsureRequestedSessionAsync(
        string? scriptId,
        IScriptRepository scriptRepository,
        IScriptSessionService sessionService)
    {
        if (string.IsNullOrWhiteSpace(scriptId))
        {
            return false;
        }

        var document = await scriptRepository.GetAsync(scriptId);
        if (document is null)
        {
            await sessionService.NewAsync();
            return false;
        }

        if (!string.Equals(sessionService.State.ScriptId, document.Id, StringComparison.Ordinal))
        {
            await sessionService.OpenAsync(document);
        }

        return true;
    }

    private static string ResolveQueryValue(string uri, string key)
    {
        var parsedUri = new Uri(uri);
        var query = parsedUri.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        var trimmedQuery = query.TrimStart('?');
        var pairs = trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 0 || !string.Equals(parts[0], key, StringComparison.Ordinal))
            {
                continue;
            }

            return parts.Length > 1
                ? Uri.UnescapeDataString(parts[1])
                : string.Empty;
        }

        return string.Empty;
    }
}
