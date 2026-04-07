namespace PrompterOne.Shared.Pages;

public sealed record TeleprompterEdgeSegmentViewModel(string Style);

public sealed record TeleprompterProgressSegmentViewModel(
    string FillStyle,
    string FillTestId,
    string Style);
