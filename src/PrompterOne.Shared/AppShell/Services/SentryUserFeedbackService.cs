using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace PrompterOne.Shared.Services;

public sealed class SentryUserFeedbackService(
    ISentryRuntimeClient sentryClient,
    RuntimeTelemetryOptions options,
    NavigationManager navigation,
    ILogger<SentryUserFeedbackService> logger)
{
    private readonly ILogger<SentryUserFeedbackService> _logger = logger;
    private readonly NavigationManager _navigation = navigation;
    private readonly RuntimeTelemetryOptions _options = options;
    private readonly ISentryRuntimeClient _sentryClient = sentryClient;

    public SentryUserFeedbackPrompt? Current { get; private set; }

    public bool IsEnabled => _options.HostEnabled && _options.SentryConfigured;

    public event EventHandler? Changed;

    public void OpenGeneralPrompt()
    {
        if (!IsEnabled)
        {
            return;
        }

        SetCurrent(new SentryUserFeedbackPrompt(
            SentryUserFeedbackPromptKind.General,
            Operation: null,
            Detail: null,
            AssociatedEventId: null));
    }

    public void OpenFatalPrompt(string operation, string detail)
    {
        if (!IsEnabled)
        {
            return;
        }

        SetCurrent(new SentryUserFeedbackPrompt(
            SentryUserFeedbackPromptKind.Fatal,
            operation,
            detail,
            AssociatedEventId: null));
    }

    public void UpdateFatalPromptAssociation(string operation, string detail, SentryId associatedEventId)
    {
        if (associatedEventId == SentryId.Empty || Current is not { Kind: SentryUserFeedbackPromptKind.Fatal } current)
        {
            return;
        }

        if (!string.Equals(current.Operation, operation, StringComparison.Ordinal) ||
            !string.Equals(current.Detail, detail, StringComparison.Ordinal) ||
            current.AssociatedEventId == associatedEventId)
        {
            return;
        }

        SetCurrent(current with { AssociatedEventId = associatedEventId });
    }

    public Task<CaptureFeedbackResult> SubmitAsync(string? name, string? email, string message)
    {
        if (!IsEnabled || Current is null)
        {
            return Task.FromResult(CaptureFeedbackResult.DisabledHub);
        }

        var normalizedMessage = message.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            return Task.FromResult(CaptureFeedbackResult.EmptyMessage);
        }

        var prompt = Current;
        var feedback = new SentryFeedback(
            normalizedMessage,
            contactEmail: Normalize(email),
            name: Normalize(name),
            url: _navigation.Uri,
            associatedEventId: prompt.AssociatedEventId);

        var feedbackId = _sentryClient.CaptureFeedback(
            feedback,
            out var result,
            scope => ApplyFeedbackScope(scope, prompt));

        LogFeedbackResult(feedbackId, prompt.Kind, result);
        if (result == CaptureFeedbackResult.Success)
        {
            Close();
        }

        return Task.FromResult(result);
    }

    public void Close()
    {
        if (Current is null)
        {
            return;
        }

        Current = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyFeedbackScope(Scope scope, SentryUserFeedbackPrompt prompt)
    {
        scope.SetTag("user.feedback.kind", prompt.Kind.ToString().ToLowerInvariant());
        scope.SetTag(
            "user.feedback.source",
            prompt.Kind == SentryUserFeedbackPromptKind.Fatal ? "fatal-error-boundary" : "settings");
        scope.SetExtra("user.feedback.url", _navigation.Uri);

        if (!string.IsNullOrWhiteSpace(prompt.Operation))
        {
            scope.SetTag("user.feedback.operation", prompt.Operation);
        }

        if (!string.IsNullOrWhiteSpace(prompt.Detail))
        {
            scope.SetExtra("user.feedback.detail", prompt.Detail);
        }

        if (prompt.AssociatedEventId is { } associatedEventId && associatedEventId != SentryId.Empty)
        {
            scope.SetTag("user.feedback.associated_event_id", associatedEventId.ToString());
        }
    }

    private void LogFeedbackResult(SentryId feedbackId, SentryUserFeedbackPromptKind kind, CaptureFeedbackResult result)
    {
        if (result == CaptureFeedbackResult.Success)
        {
            _logger.LogInformation(
                "Captured Sentry user feedback {FeedbackId} for {FeedbackKind}.",
                feedbackId,
                kind);
            return;
        }

        _logger.LogWarning(
            "Failed to capture Sentry user feedback for {FeedbackKind}. Result: {FeedbackResult}.",
            kind,
            result);
    }

    private void SetCurrent(SentryUserFeedbackPrompt next)
    {
        if (Current == next)
        {
            return;
        }

        Current = next;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
