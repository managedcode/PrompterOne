using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.Tests;

internal sealed class FakeSentryRuntimeClient : ISentryRuntimeClient
{
    public List<SentryFeedback> Feedbacks { get; } = [];

    public List<Exception> Exceptions { get; } = [];

    public List<(string Message, SentryLevel Level)> Messages { get; } = [];

    public CaptureFeedbackResult FeedbackResult { get; set; } = CaptureFeedbackResult.Success;

    public SentryId NextExceptionId { get; set; } = SentryId.Create();

    public SentryId NextFeedbackId { get; set; } = SentryId.Create();

    public SentryId NextMessageId { get; set; } = SentryId.Create();

    public SentryId CaptureMessage(string message, Action<Scope> configureScope, SentryLevel level)
    {
        Messages.Add((message, level));
        return NextMessageId;
    }

    public SentryId CaptureException(Exception exception, Action<Scope> configureScope)
    {
        Exceptions.Add(exception);
        return NextExceptionId;
    }

    public SentryId CaptureFeedback(
        SentryFeedback feedback,
        out CaptureFeedbackResult result,
        Action<Scope>? configureScope = null)
    {
        result = FeedbackResult;
        Feedbacks.Add(new SentryFeedback(
            feedback.Message,
            feedback.ContactEmail,
            feedback.Name,
            feedback.ReplayId,
            feedback.Url,
            feedback.AssociatedEventId));

        return result == CaptureFeedbackResult.Success
            ? NextFeedbackId
            : SentryId.Empty;
    }
}
