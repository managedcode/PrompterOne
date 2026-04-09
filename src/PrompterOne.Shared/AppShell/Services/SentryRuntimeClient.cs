namespace PrompterOne.Shared.Services;

public sealed class SentryRuntimeClient : ISentryRuntimeClient
{
    public SentryId CaptureMessage(string message, Action<Scope> configureScope, SentryLevel level) =>
        SentrySdk.CaptureMessage(message, configureScope, level);

    public SentryId CaptureException(Exception exception, Action<Scope> configureScope) =>
        SentrySdk.CaptureException(exception, configureScope);

    public SentryId CaptureFeedback(
        SentryFeedback feedback,
        out CaptureFeedbackResult result,
        Action<Scope>? configureScope = null)
    {
        configureScope ??= static _ => { };
        return SentrySdk.CaptureFeedback(feedback, out result, configureScope, hint: null);
    }
}
