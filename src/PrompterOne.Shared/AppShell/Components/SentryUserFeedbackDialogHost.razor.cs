using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using PrompterOne.Shared.Localization;
using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.AppShell.Components;

public class SentryUserFeedbackDialogHostBase : ComponentBase, IDisposable
{
    protected const string DialogTitleId = "feedback-dialog-title";

    [Inject] private SentryUserFeedbackService FeedbackService { get; set; } = null!;
    [Inject] private IStringLocalizer<SharedResource> Localizer { get; set; } = null!;

    protected string _email = string.Empty;
    protected bool _isSubmitting;
    protected string _errorMessage = string.Empty;
    protected string _message = string.Empty;
    protected string _name = string.Empty;
    protected SentryUserFeedbackPrompt? Prompt { get; private set; }

    protected string DialogBody =>
        Prompt?.Kind == SentryUserFeedbackPromptKind.Fatal
            ? Text(UiTextKey.FeedbackDialogFatalBody)
            : Text(UiTextKey.FeedbackDialogGeneralBody);

    protected string DialogCardClass =>
        Prompt?.Kind == SentryUserFeedbackPromptKind.Fatal
            ? "feedback-dialog-card feedback-dialog-card--fatal"
            : "feedback-dialog-card";

    protected string DialogState =>
        Prompt?.Kind == SentryUserFeedbackPromptKind.Fatal ? "fatal" : "general";

    protected string DialogTitle =>
        Prompt?.Kind == SentryUserFeedbackPromptKind.Fatal
            ? Text(UiTextKey.FeedbackDialogFatalTitle)
            : Text(UiTextKey.FeedbackDialogGeneralTitle);

    protected bool ShowContext =>
        Prompt is
        {
            Kind: SentryUserFeedbackPromptKind.Fatal,
            Operation.Length: > 0
        };

    protected string SubmitText =>
        _isSubmitting
            ? Text(UiTextKey.FeedbackDialogSubmitting)
            : Text(UiTextKey.FeedbackDialogSubmit);

    protected override void OnInitialized()
    {
        FeedbackService.Changed += HandleFeedbackChanged;
        SyncPrompt(resetForm: true);
    }

    public void Dispose()
    {
        FeedbackService.Changed -= HandleFeedbackChanged;
    }

    protected void HandleCancel()
    {
        if (_isSubmitting)
        {
            return;
        }

        FeedbackService.Close();
    }

    protected async Task HandleSubmitAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        _errorMessage = string.Empty;

        var result = await FeedbackService.SubmitAsync(_name, _email, _message);

        _isSubmitting = false;
        if (result == CaptureFeedbackResult.Success)
        {
            return;
        }

        _errorMessage = result == CaptureFeedbackResult.EmptyMessage
            ? Text(UiTextKey.FeedbackDialogMessageRequired)
            : Text(UiTextKey.FeedbackDialogError);
    }

    protected string Text(UiTextKey key) => Localizer[key.ToString()];

    private void HandleFeedbackChanged(object? sender, EventArgs args)
    {
        SyncPrompt(resetForm: ShouldResetForm(FeedbackService.Current));
        _ = InvokeAsync(StateHasChanged);
    }

    private void ResetForm()
    {
        _name = string.Empty;
        _email = string.Empty;
        _message = string.Empty;
        _errorMessage = string.Empty;
        _isSubmitting = false;
    }

    private void SyncPrompt(bool resetForm)
    {
        Prompt = FeedbackService.Current;
        if (resetForm)
        {
            ResetForm();
        }
    }

    private bool ShouldResetForm(SentryUserFeedbackPrompt? nextPrompt)
    {
        if (Prompt is null || nextPrompt is null)
        {
            return true;
        }

        return Prompt.Kind != nextPrompt.Kind ||
            !string.Equals(Prompt.Operation, nextPrompt.Operation, StringComparison.Ordinal) ||
            !string.Equals(Prompt.Detail, nextPrompt.Detail, StringComparison.Ordinal);
    }
}
