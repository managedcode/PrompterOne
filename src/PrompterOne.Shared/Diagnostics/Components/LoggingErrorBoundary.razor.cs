using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Services.Diagnostics;

namespace PrompterOne.Shared.Components.Diagnostics;

public class LoggingErrorBoundaryBase : ErrorBoundary
{
    private const string BoundaryOperation = "Unhandled UI render";

    [Inject] private UiDiagnosticsService Diagnostics { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<LoggingErrorBoundaryBase> Logger { get; set; } = null!;
    [Inject] private SentryUserFeedbackService Feedback { get; set; } = null!;

    protected UiDiagnosticEntry? CurrentDiagnostic => Diagnostics.Current;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogCritical(exception, "Unhandled UI exception reached the global error boundary.");
        Diagnostics.ReportFatal(BoundaryOperation, exception);
        return Task.CompletedTask;
    }

    protected void HandleRecover()
    {
        Feedback.Close();
        Diagnostics.Clear();
        Recover();
    }

    protected void HandleReturnHome()
    {
        Feedback.Close();
        Diagnostics.Clear();
        Recover();
        Navigation.NavigateTo("/library", replace: true);
    }
}
