using System.Globalization;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Services.Editor;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private async Task OnAiActionRequestedAsync(EditorAiAssistAction action)
    {
        var mutation = LocalAssistant.Apply(_sourceText, _selection.Range, action);
        _selection = _selection with { Range = mutation.Selection };
        _history.TryRecord(mutation.Text, mutation.Selection);
        PersistDraftInBackground(mutation.Text);

        if (_sourcePanel is not null)
        {
            await _sourcePanel.FocusRangeAsync(mutation.Selection.Start, mutation.Selection.End);
        }
    }

    private string BuildPersistedDocument(string bodyText) =>
        _frontMatterService.Build(BuildMetadata(), bodyText);

    private Dictionary<string, string> BuildMetadata()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [TpsFrontMatterDocumentService.MetadataKeys.Title] = _screenTitle,
            [TpsFrontMatterDocumentService.MetadataKeys.Author] = string.IsNullOrWhiteSpace(_author) ? DefaultAuthor : _author,
            [TpsFrontMatterDocumentService.MetadataKeys.Profile] = _profile,
            [TpsFrontMatterDocumentService.MetadataKeys.BaseWpm] = _baseWpm.ToString(CultureInfo.InvariantCulture),
            [TpsFrontMatterDocumentService.MetadataKeys.XslowOffset] = _xslowOffset.ToString(CultureInfo.InvariantCulture),
            [TpsFrontMatterDocumentService.MetadataKeys.SlowOffset] = _slowOffset.ToString(CultureInfo.InvariantCulture),
            [TpsFrontMatterDocumentService.MetadataKeys.FastOffset] = _fastOffset.ToString(CultureInfo.InvariantCulture),
            [TpsFrontMatterDocumentService.MetadataKeys.XfastOffset] = _xfastOffset.ToString(CultureInfo.InvariantCulture),
            [TpsFrontMatterDocumentService.MetadataKeys.Version] = string.IsNullOrWhiteSpace(_version) ? DefaultVersion : _version
        };

        if (!string.IsNullOrWhiteSpace(_createdDate))
        {
            metadata[TpsFrontMatterDocumentService.MetadataKeys.Created] = _createdDate;
        }

        if (!string.IsNullOrWhiteSpace(_displayDuration))
        {
            metadata[TpsFrontMatterDocumentService.MetadataKeys.DisplayDuration] = _displayDuration;
        }

        return metadata;
    }
}
