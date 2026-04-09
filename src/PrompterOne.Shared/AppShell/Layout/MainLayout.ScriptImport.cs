using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PrompterOne.Core.Abstractions;
using PrompterOne.Core.Services.Editor;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Services.Diagnostics;

namespace PrompterOne.Shared.Layout;

public partial class MainLayout
{
    private const int ImportProgressStepCount = 3;
    private const string ImportScriptMessage = "Unable to import this script.";
    private const string ImportScriptOperation = "Shell import script";
    private const string ImportScriptUnsupportedDetail = "Choose a supported script or document file such as .tps, .md, .txt, .pdf, or .docx.";
    private const long ImportScriptMaximumFileSizeBytes = 5 * 1024 * 1024;
    protected internal static string SupportedImportAcceptValue => ScriptDocumentFileTypes.PickerAcceptValue;

    private int _importScriptPickerVersion;
    private ImportProgressState? _importProgressState;

    private bool IsImportInProgress => _importProgressState is not null;

    private string ImportProgressFileName => _importProgressState?.FileName ?? string.Empty;

    private string ImportProgressLabel => _importProgressState is null
        ? string.Empty
        : Text(_importProgressState.PhaseKey);

    private string ImportProgressStepLabel => _importProgressState is null
        ? string.Empty
        : $"{_importProgressState.StepIndex}/{_importProgressState.TotalSteps}";

    private string ImportProgressWidth => _importProgressState is null
        ? "0%"
        : $"{Math.Clamp((int)Math.Round(_importProgressState.StepIndex * 100d / _importProgressState.TotalSteps), 0, 100)}%";

    [Inject] private AppShellFilePickerInterop FilePickerInterop { get; set; } = null!;
    [Inject] private IScriptRepository ScriptRepository { get; set; } = null!;
    [Inject] private ScriptDocumentImportService ScriptDocumentImportService { get; set; } = null!;
    [Inject] private UiDiagnosticsService Diagnostics { get; set; } = null!;

    private Task HandleImportScriptTriggerAsync() =>
        IsImportInProgress
            ? Task.CompletedTask
            : FilePickerInterop.OpenAsync(ImportActionInputDomId);

    private async Task HandleImportScriptAsync(InputFileChangeEventArgs args)
    {
        try
        {
            if (args.FileCount == 0)
            {
                return;
            }

            var file = args.File;
            if (!ScriptDocumentImportService.CanImport(file.Name))
            {
                Diagnostics.ReportRecoverable(ImportScriptOperation, ImportScriptMessage, ImportScriptUnsupportedDetail);
                return;
            }

            await SetImportProgressAsync(file.Name, UiTextKey.HeaderImportReading, 1);
            await Diagnostics.RunAsync(
                ImportScriptOperation,
                ImportScriptMessage,
                async () =>
                {
                    await Bootstrapper.EnsureReadyAsync();
                    await SetImportProgressAsync(file.Name, UiTextKey.HeaderImportPreparingScript, 2);
                    await using var stream = file.OpenReadStream(ImportScriptMaximumFileSizeBytes);
                    var descriptor = await ScriptDocumentImportService.ImportAsync(stream, file.Name, file.ContentType);
                    await SetImportProgressAsync(file.Name, UiTextKey.HeaderImportOpeningEditor, 3);
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
            await ClearImportProgressAsync();
            ResetImportScriptPicker();
        }
    }

    private async Task ClearImportProgressAsync()
    {
        if (_importProgressState is null)
        {
            return;
        }

        _importProgressState = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SetImportProgressAsync(string? fileName, UiTextKey phaseKey, int stepIndex)
    {
        _importProgressState = new ImportProgressState(
            FileName: ScriptDocumentFileTypes.NormalizeFileName(fileName),
            PhaseKey: phaseKey,
            StepIndex: Math.Clamp(stepIndex, 1, ImportProgressStepCount),
            TotalSteps: ImportProgressStepCount);

        await InvokeAsync(StateHasChanged);
        await Task.Yield();
    }

    private void ResetImportScriptPicker()
    {
        _importScriptPickerVersion = checked(_importScriptPickerVersion + 1);
        _ = InvokeAsync(StateHasChanged);
    }

    private sealed record ImportProgressState(
        string FileName,
        UiTextKey PhaseKey,
        int StepIndex,
        int TotalSteps);
}
