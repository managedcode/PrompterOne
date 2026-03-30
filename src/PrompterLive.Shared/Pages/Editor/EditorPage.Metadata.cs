using System.Globalization;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Components.Editor;

namespace PrompterLive.Shared.Pages;

public partial class EditorPage
{
    private async Task OnAuthorChangedAsync(string value)
    {
        _author = value.Trim();
        await PersistMetadataAsync();
    }

    private async Task OnBaseWpmChangedAsync(int value)
    {
        _baseWpm = Math.Clamp(value, 80, 600);
        await PersistMetadataAsync();
    }

    private async Task OnCreatedDateChangedAsync(string value)
    {
        _createdDate = value.Trim();
        await PersistMetadataAsync();
    }

    private async Task OnDurationChangedAsync(string value)
    {
        _displayDuration = value.Trim();
        UpdateStatus(SessionService.State);
        await PersistMetadataAsync();
    }

    private async Task OnProfileChangedAsync(string value)
    {
        _profile = string.Equals(value, DefaultProfileRsvp, StringComparison.Ordinal)
            ? DefaultProfileRsvp
            : DefaultProfileActor;
        await PersistMetadataAsync();
    }

    private async Task OnVersionChangedAsync(string value)
    {
        _version = value.Trim();
        await PersistMetadataAsync();
    }

    private async Task PersistMetadataAsync()
    {
        _history.TryRecord(_sourceText, _selection.Range);
        await PersistDraftAsync(_sourceText);
    }

    private void UpdateStatus(ScriptWorkspaceState state)
    {
        _status = new EditorStatusViewModel(
            _selection.Line,
            _selection.Column,
            _profile,
            _baseWpm,
            _segments.Count,
            _segments.Sum(segment => segment.Blocks.Count),
            state.WordCount,
            ResolveDisplayedDuration(state.EstimatedDuration),
            _version);
    }

    private string ResolveDisplayedDuration(TimeSpan estimatedDuration) =>
        string.IsNullOrWhiteSpace(_displayDuration)
            ? FormatDuration(estimatedDuration)
            : _displayDuration;

    private static string FormatDuration(TimeSpan duration)
    {
        var safeDuration = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : duration;
        return $"{(int)safeDuration.TotalMinutes}:{safeDuration.Seconds:00}";
    }

    private static string GetMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        return fallback;
    }

    private static int TryGetInt(IReadOnlyDictionary<string, string> metadata, string key, int fallback) =>
        metadata.TryGetValue(key, out var value) && int.TryParse(value, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
}
