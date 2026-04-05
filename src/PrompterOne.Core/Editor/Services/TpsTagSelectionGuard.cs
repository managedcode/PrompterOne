using System.Text.RegularExpressions;
using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Core.Services.Editor;

internal static partial class TpsTagSelectionGuard
{
    [GeneratedRegex(@"\[[^\]\r\n]+\]", RegexOptions.CultureInvariant)]
    private static partial Regex TagTokenRegex();

    internal static bool TouchesTagSyntax(string text, EditorSelectionRange selection)
    {
        if (string.IsNullOrEmpty(text) || !selection.HasSelection)
        {
            return false;
        }

        foreach (Match match in TagTokenRegex().Matches(text))
        {
            if (selection.OrderedStart < match.Index + match.Length &&
                match.Index < selection.OrderedEnd)
            {
                return true;
            }
        }

        return false;
    }
}
