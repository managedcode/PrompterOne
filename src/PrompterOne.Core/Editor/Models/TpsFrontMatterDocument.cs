namespace PrompterOne.Core.Models.Editor;

public sealed record TpsFrontMatterDocument(
    IReadOnlyDictionary<string, string> Metadata,
    string Body,
    int BodyStartIndex);
