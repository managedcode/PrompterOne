using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PrompterOne.Core.Abstractions;
using PrompterOne.Core.Services.Editor;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Services.Diagnostics;

namespace PrompterOne.Shared.Layout;

public partial class MainLayout
{
    private const string OpenScriptMessage = "Unable to import this script.";
    private const string OpenScriptOperation = "Library open script";
    private const string OpenScriptUnsupportedDetail = "Choose a .tps, .tps.md, .md.tps, .md, or .txt file.";
    private const long OpenScriptMaximumFileSizeBytes = 5 * 1024 * 1024;
    protected internal const string SupportedOpenScriptAcceptValue = ".tps,.tps.md,.md.tps,.md,.txt";

    private int _openScriptPickerVersion;

    [Inject] private AppShellFilePickerInterop FilePickerInterop { get; set; } = null!;
    [Inject] private IScriptRepository ScriptRepository { get; set; } = null!;
    [Inject] private ScriptImportDescriptorService ScriptImportDescriptorService { get; set; } = null!;
    [Inject] private UiDiagnosticsService Diagnostics { get; set; } = null!;

    private Task HandleOpenScriptTriggerAsync() =>
        FilePickerInterop.OpenAsync(UiDomIds.AppShell.LibraryOpenScriptInput);

    private async Task HandleOpenScriptAsync(InputFileChangeEventArgs args)
    {
        try
        {
            if (args.FileCount == 0)
            {
                return;
            }

            var file = args.File;
            if (!ScriptImportDescriptorService.CanImport(file.Name))
            {
                Diagnostics.ReportRecoverable(OpenScriptOperation, OpenScriptMessage, OpenScriptUnsupportedDetail);
                return;
            }

            await Diagnostics.RunAsync(
                OpenScriptOperation,
                OpenScriptMessage,
                async () =>
                {
                    await Bootstrapper.EnsureReadyAsync();
                    var importedText = await ReadImportedTextAsync(file);
                    var descriptor = ScriptImportDescriptorService.Build(file.Name, importedText);
                    var document = await ScriptRepository.SaveAsync(
                        descriptor.Title,
                        descriptor.Text,
                        descriptor.DocumentName);

                    await SessionService.OpenAsync(document);
                    Navigation.NavigateTo(AppRoutes.EditorWithId(document.Id));
                });
        }
        finally
        {
            ResetOpenScriptPicker();
        }
    }

    private static async Task<string> ReadImportedTextAsync(IBrowserFile file)
    {
        await using var stream = file.OpenReadStream(OpenScriptMaximumFileSizeBytes);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    private void ResetOpenScriptPicker()
    {
        _openScriptPickerVersion = checked(_openScriptPickerVersion + 1);
        _ = InvokeAsync(StateHasChanged);
    }
}
