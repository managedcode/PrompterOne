namespace PrompterOne.Core.Models.Workspace;

public sealed record GoLiveDestinationSourceSelection(
    string TargetId,
    IReadOnlyList<string> SourceIds);
