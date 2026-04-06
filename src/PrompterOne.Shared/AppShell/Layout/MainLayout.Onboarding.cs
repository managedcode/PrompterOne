using Microsoft.AspNetCore.Components;
using PrompterOne.Core.Abstractions;
using PrompterOne.Shared.Components;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Layout;

public partial class MainLayout
{
    private IReadOnlyList<AppOnboardingStepDescriptor> _onboardingSteps = [];
    private bool _showOnboarding;
    private int _onboardingStepIndex;

    [Inject] private IUserSettingsStore SettingsStore { get; set; } = null!;
    private bool ShowOnboarding => _showOnboarding && _onboardingSteps.Count > 0;

    private async Task InitializeOnboardingAsync()
    {
        var preferences = await SettingsStore.LoadAsync<SettingsPagePreferences>(SettingsPagePreferences.StorageKey)
            ?? SettingsPagePreferences.Default;
        if (preferences.HasSeenOnboarding)
        {
            _showOnboarding = false;
            return;
        }

        var featuredScriptId = await ResolveOnboardingScriptIdAsync();
        _onboardingSteps = AppOnboardingCatalog.Build(featuredScriptId);
        _onboardingStepIndex = ResolveOnboardingStepIndex(Navigation.Uri);
        _showOnboarding = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleOnboardingDismissAsync()
    {
        await PersistOnboardingSeenAsync();
        _showOnboarding = false;
        await InvokeAsync(StateHasChanged);
    }

    private Task HandleOnboardingBackAsync() =>
        NavigateOnboardingAsync(_onboardingStepIndex - 1);

    private async Task HandleOnboardingNextAsync()
    {
        if (_onboardingStepIndex >= _onboardingSteps.Count - 1)
        {
            await PersistOnboardingSeenAsync();
            _showOnboarding = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        await NavigateOnboardingAsync(_onboardingStepIndex + 1);
    }

    private Task HandleOnboardingStepSelectedAsync(AppOnboardingStepId stepId)
    {
        var targetIndex = _onboardingSteps
            .Select((step, index) => (step, index))
            .FirstOrDefault(candidate => candidate.step.StepId == stepId)
            .index;

        return NavigateOnboardingAsync(targetIndex);
    }

    private async Task NavigateOnboardingAsync(int targetIndex)
    {
        if (_onboardingSteps.Count == 0)
        {
            return;
        }

        _onboardingStepIndex = Math.Clamp(targetIndex, 0, _onboardingSteps.Count - 1);
        var targetRoute = _onboardingSteps[_onboardingStepIndex].Route;
        if (!string.Equals(GetNormalizedAppRoute(Navigation.Uri), targetRoute, StringComparison.Ordinal))
        {
            Navigation.NavigateTo(targetRoute);
            return;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void SyncOnboardingStepWithCurrentRoute(string uri)
    {
        if (!ShowOnboarding)
        {
            return;
        }

        var matchingIndex = ResolveOnboardingStepIndex(uri);
        if (matchingIndex == _onboardingStepIndex)
        {
            return;
        }

        _onboardingStepIndex = matchingIndex;
    }

    private int ResolveOnboardingStepIndex(string uri)
    {
        if (_onboardingSteps.Count == 0)
        {
            return 0;
        }

        var currentRoute = GetNormalizedAppRoute(uri);
        var routeIndex = _onboardingSteps
            .Select((step, index) => (step.Route, index))
            .FirstOrDefault(candidate => string.Equals(candidate.Route, currentRoute, StringComparison.Ordinal))
            .index;

        return routeIndex;
    }

    private async Task<string> ResolveOnboardingScriptIdAsync()
    {
        if (!string.IsNullOrWhiteSpace(ShellState.ScriptId))
        {
            return ShellState.ScriptId;
        }

        var summaries = await ScriptRepository.ListAsync();
        return summaries
            .OrderBy(summary => summary.UpdatedAt)
            .Select(summary => summary.Id)
            .FirstOrDefault() ?? string.Empty;
    }

    private async Task PersistOnboardingSeenAsync()
    {
        var preferences = await SettingsStore.LoadAsync<SettingsPagePreferences>(SettingsPagePreferences.StorageKey)
            ?? SettingsPagePreferences.Default;
        if (preferences.HasSeenOnboarding)
        {
            return;
        }

        await SettingsStore.SaveAsync(
            SettingsPagePreferences.StorageKey,
            preferences with { HasSeenOnboarding = true });
    }

    private static string GetNormalizedAppRoute(string uri)
    {
        var parsedUri = new Uri(uri);
        var normalizedPath = parsedUri.AbsolutePath.TrimEnd('/');
        var route = string.IsNullOrWhiteSpace(normalizedPath)
            ? AppRoutes.Library
            : normalizedPath;

        return string.IsNullOrWhiteSpace(parsedUri.Query)
            ? route
            : string.Concat(route, parsedUri.Query);
    }
}
