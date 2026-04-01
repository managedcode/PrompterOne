namespace PrompterOne.Shared.Services.Diagnostics;

public sealed record UiDiagnosticEntry(
    string Title,
    string Message,
    string Operation,
    string Detail,
    bool IsFatal,
    DateTimeOffset OccurredAtUtc);
