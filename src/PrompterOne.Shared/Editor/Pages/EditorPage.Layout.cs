namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private const string CollapseMetadataRailLabel = "Collapse metadata panel";
    private const string ExpandMetadataRailLabel = "Expand metadata panel";

    private bool _isMetadataRailCollapsed;

    private Task OnMetadataRailToggleAsync()
    {
        _isMetadataRailCollapsed = !_isMetadataRailCollapsed;
        return Task.CompletedTask;
    }

    private string GetMetadataRailToggleLabel() =>
        _isMetadataRailCollapsed ? ExpandMetadataRailLabel : CollapseMetadataRailLabel;
}
