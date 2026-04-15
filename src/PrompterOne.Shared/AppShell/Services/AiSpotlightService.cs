using Microsoft.Extensions.Localization;
using PrompterOne.Core.AI.Abstractions;
using PrompterOne.Core.AI.Models;
using PrompterOne.Core.AI.Services;
using PrompterOne.Core.AI.Workflows;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;
using PrompterOne.Shared.Tools;

namespace PrompterOne.Shared.Services;

public sealed class AiSpotlightService(
    ScriptDocumentEditService documentEditService,
    IScriptAgentRuntime scriptAgentRuntime,
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
                AgentOutput = null,
                ErrorMessage = null
            });

            return Task.CompletedTask;
        }

        return RunAgentAsync(State.Prompt.Trim());
    }

    private async Task RunAgentAsync(string prompt)
    {
        var context = AttachAvailableTools(State.Context);
        SetState(State with
        {
            IsOpen = true,
            Mode = AiSpotlightMode.Running,
            Context = context,
            Log = AiSpotlightExecutionBuilder.BuildAgentRunningLog(context, Text, Format),
            RequiresApproval = false,
            ApprovalRequest = null,
            AgentOutput = null,
            ErrorMessage = null
        });

        try
        {
            var result = await scriptAgentRuntime.RunAsync(
                new ScriptAgentRunRequest(
                    AssistantScriptWorkflow.WorkflowId,
                    prompt,
                    ConversationId: CreateConversationId(context),
                    ArticleContext: context));
            SetState(State with
            {
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightAgentResponse), result.Output, true)),
                AgentOutput = result.Output,
                ErrorMessage = null
            });

            await ApplyStructuredDocumentEditsAsync(result, context);
        }
        catch (InvalidOperationException exception)
        {
            SetState(State with
            {
                ErrorMessage = Text(UiTextKey.AiSpotlightAgentUnavailable),
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(
                        Text(UiTextKey.AiSpotlightAgentUnavailable),
                        string.IsNullOrWhiteSpace(exception.Message)
                            ? Text(UiTextKey.AiSpotlightAgentUnavailableDetail)
                            : exception.Message))
            });
        }
        catch (NotSupportedException exception)
        {
            SetState(State with
            {
                ErrorMessage = Text(UiTextKey.AiSpotlightAgentFailed),
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightAgentFailed), exception.Message))
            });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SetState(State with
            {
                ErrorMessage = Text(UiTextKey.AiSpotlightAgentFailed),
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightAgentFailed), exception.Message))
            });
        }
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
                AgentOutput = null,
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

    private async Task ApplyStructuredDocumentEditsAsync(ScriptAgentRunResult result, ScriptArticleContext context)
    {
        var edits = result.StructuredOutput?.DocumentEdits;
        if (edits is null || edits.Count == 0)
        {
            return;
        }

        if (_documentEditHandler is null || context.Editor is null)
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
            var sourceText = context.Editor.Content ?? string.Empty;
            var operations = CreateValidatedStructuredOperations(edits, sourceText);
            if (operations.Length == 0)
            {
                return;
            }

            var revision = context.Editor.Revision ?? ScriptDocumentRevision.Create(sourceText);
            var plan = new ScriptDocumentEditPlan(revision, operations, context.Editor.DocumentId);
            var applied = await _documentEditHandler(plan);
            SetState(State with
            {
                Log = AiSpotlightExecutionBuilder.AddLog(
                    State.Log,
                    new AiSpotlightLogEntry(
                        Text(UiTextKey.AiSpotlightAppliedEdit),
                        Format(UiTextKey.AiSpotlightDocumentRevisionFormat, applied.Revision.Value[..8]),
                        true)),
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
                    new AiSpotlightLogEntry(Text(UiTextKey.AiSpotlightAgentFailed), exception.Message))
            });
        }
    }

    private ScriptDocumentEditOperation[] CreateValidatedStructuredOperations(
        IEnumerable<ScriptAgentStructuredEdit> edits,
        string sourceText)
    {
        var operations = new List<ScriptDocumentEditOperation>();
        foreach (var edit in edits)
        {
            var operation = edit.ToDocumentEditOperation();
            if (operation is null)
            {
                continue;
            }

            ValidateStructuredEdit(operation, edit.ExpectedText, sourceText);
            operations.Add(operation);
        }

        return [.. operations];
    }

    private void ValidateStructuredEdit(
        ScriptDocumentEditOperation operation,
        string? expectedText,
        string sourceText)
    {
        operation.Range.ValidateWithin(sourceText.Length);
        if (operation.Kind is not (ScriptDocumentEditKind.Replace or ScriptDocumentEditKind.Delete))
        {
            return;
        }

        if (expectedText is null)
        {
            throw new InvalidOperationException(Text(UiTextKey.AiSpotlightStructuredEditMissingExpectedText));
        }

        var actualText = sourceText.Substring(operation.Range.Start, operation.Range.Length);
        if (!string.Equals(actualText, expectedText, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(Text(UiTextKey.AiSpotlightStructuredEditRangeMismatch));
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
            AgentOutput = null,
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

    private static string? CreateConversationId(ScriptArticleContext context) =>
        context.Editor?.DocumentId ?? context.Route ?? context.Screen;
}
