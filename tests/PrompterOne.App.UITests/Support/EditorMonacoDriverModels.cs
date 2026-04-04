namespace PrompterOne.App.UITests;

internal sealed class EditorMonacoState
{
    public List<string> DecorationClasses { get; set; } = [];

    public string Engine { get; set; } = string.Empty;

    public string LanguageId { get; set; } = string.Empty;

    public int LineCount { get; set; }

    public bool Ready { get; set; }

    public double ScrollTop { get; set; }

    public EditorMonacoSelection Selection { get; set; } = new();

    public string Text { get; set; } = string.Empty;
}

internal sealed class EditorMonacoSelection
{
    public int Column { get; set; }

    public int End { get; set; }

    public int Line { get; set; }

    public int Start { get; set; }

    public double? ToolbarLeft { get; set; }

    public double? ToolbarTop { get; set; }
}
