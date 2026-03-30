using System.Text.RegularExpressions;

namespace PrompterLive.App.Tests;

public sealed class EditorStylesheetContractTests
{
    private const string EditorSupportNamespace = "EditorSurfaceInterop";
    private const string HighlightAndInputRule = ".ed-source-highlight,\n.ed-source-input";
    private static readonly string ComponentStylesheetPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/Editor/Components/EditorSourcePanel.razor.css"));
    private static readonly string EditorSupportScriptPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/wwwroot/editor/editor-source-panel.js"));
    private static readonly string BrowserScriptPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/wwwroot/prompterlive-browser.js"));
    private static readonly string ShellScriptPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/wwwroot/prompterlive-shell.js"));
    private static readonly string HostIndexPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.App/wwwroot/index.html"));
    private static readonly string InteropPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/Editor/Services/EditorInterop.cs"));
    private static readonly string SharedStylesheetPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "../../../../../src/PrompterLive.Shared/wwwroot/design/styles.css"));

    [Fact]
    public void EditorSurface_UsesSharedMetricSafeTypographyForHighlightAndInput()
    {
        var normalizedRule = NormalizeCssRule(GetRuleBlock(HighlightAndInputRule));

        Assert.Contains("font-size:16px;", normalizedRule, StringComparison.Ordinal);
        Assert.Contains("line-height:2;", normalizedRule, StringComparison.Ordinal);
        Assert.Contains("letter-spacing:normal;", normalizedRule, StringComparison.Ordinal);
        Assert.Contains("font-variant-ligatures:none;", normalizedRule, StringComparison.Ordinal);
        Assert.Contains("white-space:pre-wrap;", normalizedRule, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap:anywhere;", normalizedRule, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".ed-main ::deep .ed-src-line-segment")]
    [InlineData(".ed-main ::deep .ed-src-line-block")]
    [InlineData(".ed-main ::deep .mk-pause")]
    [InlineData(".ed-main ::deep .mk-hl")]
    [InlineData(".ed-main ::deep .mk-phonetic")]
    [InlineData(".ed-main ::deep .mk-special")]
    [InlineData(".ed-main ::deep .mk-edit")]
    public void EditorHighlightRules_DoNotChangeTextMetrics(string selector)
    {
        var rule = GetRuleBlock(selector);

        Assert.DoesNotContain("font-size", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("margin", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("padding", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("letter-spacing", rule, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".ed-main ::deep .h-mark", "font-size")]
    [InlineData(".ed-main ::deep .h-sep", "margin")]
    public void HeaderTokenRules_AvoidSyntheticSpacing(string selector, string forbiddenDeclaration)
    {
        var rule = GetRuleBlock(selector);

        Assert.DoesNotContain(forbiddenDeclaration, rule, StringComparison.Ordinal);
    }

    [Fact]
    public void EditorSupportAssets_AreIsolatedFromGlobalShellAssets()
    {
        var componentStylesheet = File.ReadAllText(ComponentStylesheetPath);
        var globalStylesheet = File.ReadAllText(SharedStylesheetPath);
        var editorSupportScript = File.ReadAllText(EditorSupportScriptPath);
        var browserScript = File.ReadAllText(BrowserScriptPath);
        var shellScript = File.ReadAllText(ShellScriptPath);
        var hostIndex = File.ReadAllText(HostIndexPath);
        var interopSource = File.ReadAllText(InteropPath);

        Assert.Contains(".tb-btn", componentStylesheet, StringComparison.Ordinal);
        Assert.Contains(".ed-float-bar", componentStylesheet, StringComparison.Ordinal);
        Assert.Contains(".ed-main ::deep .ed-src-line-segment", componentStylesheet, StringComparison.Ordinal);

        Assert.DoesNotContain(".tb-btn", globalStylesheet, StringComparison.Ordinal);
        Assert.DoesNotContain(".ed-float-bar", globalStylesheet, StringComparison.Ordinal);
        Assert.DoesNotContain(".ed-source-highlight", globalStylesheet, StringComparison.Ordinal);

        Assert.Contains(EditorSupportNamespace, interopSource, StringComparison.Ordinal);
        Assert.Contains($"editorSurfaceNamespace = \"{EditorSupportNamespace}\"", editorSupportScript, StringComparison.Ordinal);
        Assert.Contains($"window[{EditorSupportNamespace.ToLowerInvariant() switch {{ _ => "\"EditorSurfaceInterop\"" }}]", editorSupportScript, StringComparison.Ordinal);
        Assert.DoesNotContain("editor:", browserScript, StringComparison.Ordinal);
        Assert.DoesNotContain("editor:", shellScript, StringComparison.Ordinal);

        Assert.Contains("PrompterLive.App.styles.css", hostIndex, StringComparison.Ordinal);
        Assert.Contains("_content/PrompterLive.Shared/editor/editor-source-panel.js", hostIndex, StringComparison.Ordinal);
        Assert.Contains("_content/PrompterLive.Shared/prompterlive-shell.js", hostIndex, StringComparison.Ordinal);
        Assert.Contains("_content/PrompterLive.Shared/prompterlive-browser.js", hostIndex, StringComparison.Ordinal);
    }

    private static string GetRuleBlock(string selector)
    {
        var stylesheet = File.ReadAllText(ComponentStylesheetPath);
        var selectorPattern = string.Join(
            "\\s*",
            selector.Split('\n', StringSplitOptions.TrimEntries)
                .Select(Regex.Escape));
        var pattern = $"{selectorPattern}\\s*\\{{(?<body>.*?)\\}}";
        var match = Regex.Match(stylesheet, pattern, RegexOptions.Singleline);

        Assert.True(match.Success, $"Could not find CSS rule for selector '{selector}'.");
        return match.Groups["body"].Value;
    }

    private static string NormalizeCssRule(string rule) =>
        string.Concat(rule.Where(character => !char.IsWhiteSpace(character)));
}
