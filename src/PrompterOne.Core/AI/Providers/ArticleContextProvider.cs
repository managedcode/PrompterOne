using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Providers;

public sealed class ArticleContextProvider(ScriptArticleContext? articleContext)
    : AIContextProvider(PassThroughMessages, PassThroughMessages, PassThroughMessages)
{
    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(articleContext is null || articleContext.IsEmpty
            ? new AIContext()
            : new AIContext
            {
                Instructions = BuildInstructions(articleContext)
            });
    }

    private static string BuildInstructions(ScriptArticleContext articleContext)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Article context is available for this run.");

        if (!string.IsNullOrWhiteSpace(articleContext.Title))
        {
            builder.Append("Title: ");
            builder.AppendLine(articleContext.Title.Trim());
        }

        if (!string.IsNullOrWhiteSpace(articleContext.Source))
        {
            builder.Append("Source: ");
            builder.AppendLine(articleContext.Source.Trim());
        }

        if (!string.IsNullOrWhiteSpace(articleContext.Summary))
        {
            builder.AppendLine();
            builder.AppendLine("Summary:");
            builder.AppendLine(articleContext.Summary.Trim());
        }

        if (!string.IsNullOrWhiteSpace(articleContext.Content))
        {
            builder.AppendLine();
            builder.AppendLine("Article content:");
            builder.AppendLine(articleContext.Content.Trim());
        }

        return builder.ToString().Trim();
    }

    private static IEnumerable<ChatMessage> PassThroughMessages(IEnumerable<ChatMessage> messages) => messages;
}
