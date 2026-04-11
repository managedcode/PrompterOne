namespace PrompterOne.Core.AI.Models;

public sealed record ScriptArticleContext(
    string? Title = null,
    string? Summary = null,
    string? Content = null,
    string? Source = null)
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Title)
        && string.IsNullOrWhiteSpace(Summary)
        && string.IsNullOrWhiteSpace(Content)
        && string.IsNullOrWhiteSpace(Source);
}
