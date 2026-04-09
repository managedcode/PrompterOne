namespace PrompterOne.Shared.Services;

public enum SentryUserFeedbackPromptKind
{
    General = 0,
    Fatal = 1
}

public sealed record SentryUserFeedbackPrompt(
    SentryUserFeedbackPromptKind Kind,
    string? Operation,
    string? Detail,
    SentryId? AssociatedEventId);
