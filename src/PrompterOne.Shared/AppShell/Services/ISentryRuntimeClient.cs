namespace PrompterOne.Shared.Services;

public interface ISentryRuntimeClient
{
    SentryId CaptureMessage(string message, Action<Scope> configureScope, SentryLevel level);

    SentryId CaptureException(Exception exception, Action<Scope> configureScope);

    SentryId CaptureFeedback(
        SentryFeedback feedback,
        out CaptureFeedbackResult result,
        Action<Scope>? configureScope = null);
}
