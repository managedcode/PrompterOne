using Microsoft.Extensions.Localization;
using PrompterOne.Core.AI.Models;
using PrompterOne.Core.AI.Services;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;
using PrompterOne.Shared.Tools;

namespace PrompterOne.Shared.Services;

public sealed class AiSpotlightService(
    ScriptDocumentEditService documentEditService,
    IStringLocalizer<SharedResource> localizer)
{
    public event Action? StateChanged;

    private Func<ScriptDocumentEditPlan, Task<ScriptDocumentEditResult>>? _documentEditHandler;

    public AiSpotlightState State { get; private set; } = AiSpotlightState.Closed;

    public void SetRouteContext(AppShellScreen screen, string route, string title)
    {
        var existing = State.Context;
        var next = existing.Editor is not null && screen == AppShellScreen.Editor
            ? existing with { Title = title, Source = "PrompterOne", Route = route, Screen = screen.ToString() }
            : new ScriptArticleContext(Title: title, Source: "PrompterOne", Route: route, Screen: screen.ToString());

        SetContext(next);
    }

    public IDisposable RegisterDocumentEditTarget(Func<ScriptDocumentEditPlan, Task<ScriptDocumentEditResult>> applyAsync)
    {
        ArgumentNullException.ThrowIfNull(applyAsync);
        _documentEditHandler = applyAsync;
        return new CallbackDisposable(() =>
        {
            if (_documentEditHandler == applyAsync)
            {
                _documentEditHandler = null;
            }
        });
    }

    public void SetContext(ScriptArticleContext context) =>
        SetState(State with { Context = AttachAvailableTools(context) });

    public void Open() =>
        SetState(State with { IsOpen = true });

    public void Toggle()
    {
        if (State.IsOpen)
        {
            Close();
            return;
        }

        Open();
    }

    public void Close() =>
        SetState(State with { IsOpen = false });

    public void UpdatePrompt(string prompt) =>
        SetState(State with { Prompt = prompt ?? string.Empty });

    public Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(State.Prompt))
        {
            return Task.CompletedTask;
        }

        return ExecuteAsync();
    }

    public Task ExecuteAsync()
    {
        if (AiSpotlightApprovalBuilder.TryCreate(
                State,
                documentEditService,
                out var approvalRequest) &&
            approvalRequest is not null)
        {
            SetState(State with
            {
                IsOpen = true,
                Mode = AiSpotlightMode.Approval,
                Log = AiSpotlightExecutionBuilder.BuildApprovalLog(approvalRequest, Text, Format),
                RequiresApproval = true,
                ApprovalRequest = approvalRequest,
                ErrorMessage = null
            });

            return Task.CompletedTask;
        }

        SetState(State with
        {
            IsOpen = true,
            Mode = AiSpotlightMode.Running,
            Log = AiSpotlightExecutionBuilder.BuildRunningLog(State.Context, Text, Format),
            RequiresApproval = false,
            ApprovalRequest = null,
            ErrorMessage = null
        });

        return Task.CompletedTask;
    }

    public async Task ApproveAsync()
    {
        if (State.ApprovalRequest is not { } approvalRequest)
        {
            return;
        }

        if (_documentEditHandler is null)
        {
            SetState(State with
            {
                ErrorMessage = Text(UiTextKey.AiSpotlightNoActiveEditorTarget),
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightApprovalFailed), Text(UiTextKey.AiSpotlightOpenEditorTryAgain)))
            });
            return;
        }

        try
        {
            var result = await _documentEditHandler(approvalRequest.Plan);
            SetState(State with
            {
                Mode = AiSpotlightMode.Running,
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightAppliedEdit), Format(UiTextKey.AiSpotlightDocumentRevisionFormat, result.Revision.Value[..8]), true)),
                RequiresApproval = false,
                ApprovalRequest = null,
                ErrorMessage = null
            });
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentOutOfRangeException)
        {
            SetState(State with
            {
                ErrorMessage = exception.Message,
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightApprovalFailed), exception.Message))
            });
        }
    }

    public void RejectApproval()
    {
        SetState(State with
        {
            Mode = AiSpotlightMode.Running,
            Log = AiSpotlightExecutionBuilder.AddLog(
                State.Log,
                new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightChangeRejected), Text(UiTextKey.AiSpotlightNoDocumentTextChanged), true)),
            RequiresApproval = false,
            ApprovalRequest = null,
            ErrorMessage = null
        });
    }

    private void SetState(AiSpotlightState state)
    {
        if (EqualityComparer<AiSpotlightState>.Default.Equals(State, state))
        {
            return;
        }

        State = state;
        StateChanged?.Invoke();
    }

    private ScriptArticleContext AttachAvailableTools(ScriptArticleContext context)
    {
        var tools = AiSpotlightToolCatalog.BuildAgentTools(context)
            .Select(tool => tool.ToAgentTool(Text))
            .ToArray();

        return context with { AvailableTools = tools };
    }

    private string Text(UiTextKey key) => localizer[key.ToString()];

    private string Format(UiTextKey key, params object[] arguments) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Text(key), arguments);
}
