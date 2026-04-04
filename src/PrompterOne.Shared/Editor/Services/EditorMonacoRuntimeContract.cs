namespace PrompterOne.Shared.Services.Editor;

public static class EditorMonacoRuntimeContract
{
    public const string BrowserHarnessGlobalName = "__prompterOneEditorHarness";
    public const string EditorEngineAttributeName = "data-editor-engine";
    public const string EditorEngineAttributeValue = "monaco";
    public const string EditorProxyChangedEventName = "prompterone:editor-proxy-changed";
    public const string EditorReadyAttributeName = "data-editor-ready";
    public const string MonacoBasePath = "./_content/PrompterOne.Shared/vendor/monaco-editor/v0.55.1/min";
    public const string MonacoStylesheetPath = MonacoBasePath + "/vs/editor/editor.main.css";
    public const string MonacoVsPath = MonacoBasePath + "/vs";
    public const string MonacoLoaderPath = MonacoVsPath + "/loader.js";
    public const string TpsLanguageId = "prompter-one-tps";
    public const string DarkThemeName = "prompter-one-tps-dark";
    public const string LightThemeName = "prompter-one-tps-light";
}
