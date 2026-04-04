using System.Text.Json;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class EditorFileSaveFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private const string DownloadMode = "download";
    private const string EditedScript =
        """
        ## [Launch Angle|205WPM|focused|1:00-2:00]
        ### [Signal Block|205WPM|professional]
        This saved draft proves the file export path. / [highlight]Keep the styling[/highlight] //
        """;
    private const string ExpectedDocumentName = "test-quantum-computing.tps";
    private const string FilePickerMode = "file-system";
    private const string HarnessDisableSavePickerScript =
        "() => window.__prompterOneEditorFileSaveHarness.disableSavePicker()";
    private const string HarnessGetSavedFileStateScript =
        "() => window.__prompterOneEditorFileSaveHarness.getSavedFileState()";
    private const string HarnessResetScript =
        "() => window.__prompterOneEditorFileSaveHarness.reset()";
    private const int SavedFilePollAttempts = 20;
    private const int SavedFilePollDelayMs = 50;
    private const string SavedFileTitleLine = "title: \"Quantum Computing\"";

    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_SaveFile_UsesFilePickerAndWritesCanonicalTpsDocument()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.AddInitScriptAsync(scriptPath: GetEditorFileSaveHarnessScriptPath());
            await page.GotoAsync(BrowserTestConstants.Routes.EditorQuantum);
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, EditedScript);

            await Expect(page.GetByTestId(UiTestIds.Header.EditorSaveFile)).ToBeVisibleAsync();
            await page.GetByTestId(UiTestIds.Header.EditorSaveFile).ClickAsync();
            var savedFile = await WaitForSavedFileAsync(page, FilePickerMode);
            var savedText = savedFile.GetProperty("text").GetString() ?? string.Empty;

            Assert.Equal(FilePickerMode, savedFile.GetProperty("mode").GetString());
            Assert.Equal(ExpectedDocumentName, savedFile.GetProperty("fileName").GetString());
            Assert.Equal(1, savedFile.GetProperty("pickerCallCount").GetInt32());
            Assert.True(savedFile.GetProperty("hasBlob").GetBoolean());
            Assert.StartsWith("---", savedText, StringComparison.Ordinal);
            Assert.Contains(SavedFileTitleLine, savedText, StringComparison.Ordinal);
            Assert.Contains(EditedScript, savedText, StringComparison.Ordinal);
            await Expect(page.GetByTestId(UiTestIds.Editor.SourceInput)).ToHaveValueAsync(EditedScript);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_SaveFile_FallsBackToDownloadWhenSavePickerIsUnavailable()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.AddInitScriptAsync(scriptPath: GetEditorFileSaveHarnessScriptPath());
            await page.GotoAsync(BrowserTestConstants.Routes.EditorQuantum);
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await page.EvaluateAsync(HarnessResetScript);
            await page.EvaluateAsync(HarnessDisableSavePickerScript);
            await EditorMonacoDriver.SetTextAsync(page, EditedScript);

            await page.GetByTestId(UiTestIds.Header.EditorSaveFile).ClickAsync();
            var savedFile = await WaitForSavedFileAsync(page, DownloadMode);
            var savedText = savedFile.GetProperty("text").GetString() ?? string.Empty;

            Assert.Equal(DownloadMode, savedFile.GetProperty("mode").GetString());
            Assert.Equal(0, savedFile.GetProperty("pickerCallCount").GetInt32());
            Assert.Equal(1, savedFile.GetProperty("downloadCallCount").GetInt32());
            Assert.Equal(ExpectedDocumentName, savedFile.GetProperty("fileName").GetString());
            Assert.Contains(EditedScript, savedText, StringComparison.Ordinal);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static string GetEditorFileSaveHarnessScriptPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../tests/PrompterOne.App.UITests/Editor/editor-file-save-harness.js"));

    private static async Task<JsonElement> WaitForSavedFileAsync(Microsoft.Playwright.IPage page, string mode)
    {
        JsonElement savedFile = default;

        for (var attempt = 0; attempt < SavedFilePollAttempts; attempt++)
        {
            savedFile = await page.EvaluateAsync<JsonElement>(HarnessGetSavedFileStateScript);
            if (savedFile.GetProperty("hasBlob").GetBoolean()
                && string.Equals(mode, savedFile.GetProperty("mode").GetString(), StringComparison.Ordinal))
            {
                return savedFile;
            }

            await page.WaitForTimeoutAsync(SavedFilePollDelayMs);
        }

        return savedFile;
    }
}
