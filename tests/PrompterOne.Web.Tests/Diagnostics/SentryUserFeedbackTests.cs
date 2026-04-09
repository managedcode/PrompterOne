using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using PrompterOne.Shared.AppShell.Components;
using PrompterOne.Shared.Components.Diagnostics;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Tests;

namespace PrompterOne.Web.Tests;

[NotInParallel]
public sealed class SentryUserFeedbackTests : BunitContext
{
    private readonly AppHarness _harness;

    public SentryUserFeedbackTests()
    {
        _harness = TestHarnessFactory.Create(
            this,
            runtimeTelemetryOptions: CreateTelemetryOptions());
    }

    [Test]
    public void SettingsAboutSection_ShareFeedbackButton_OpensFeedbackDialog()
    {
        var cut = Render<SettingsPage>();
        var dialogHost = Render<SentryUserFeedbackDialogHost>();

        cut.FindByTestId(UiTestIds.Settings.NavAbout).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.FindByTestId(UiTestIds.Settings.AboutFeedbackCard));
            Assert.NotNull(cut.FindByTestId(UiTestIds.Settings.AboutFeedbackOpen));
        });

        cut.FindByTestId(UiTestIds.Settings.AboutFeedbackOpen).Click();

        dialogHost.WaitForAssertion(() =>
        {
            Assert.NotNull(dialogHost.FindByTestId(UiTestIds.Feedback.Dialog));

            var feedback = Services.GetRequiredService<SentryUserFeedbackService>();
            Assert.NotNull(feedback.Current);
            Assert.Equal(SentryUserFeedbackPromptKind.General, feedback.Current!.Kind);
        });
    }

    [Test]
    public void FeedbackDialogHost_Submit_CapturesFeedbackThroughSentry()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/settings");

        var feedback = Services.GetRequiredService<SentryUserFeedbackService>();
        feedback.OpenGeneralPrompt();

        var cut = Render<SentryUserFeedbackDialogHost>();

        cut.FindByTestId(UiTestIds.Feedback.Name).Input("Anna Petrenko");
        cut.FindByTestId(UiTestIds.Feedback.Email).Input("anna@example.com");
        cut.FindByTestId(UiTestIds.Feedback.Message).Input("The settings layout needs a clearer success state.");
        cut.FindByTestId(UiTestIds.Feedback.Submit).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Null(feedback.Current);

            var capturedFeedback = Assert.Single(_harness.SentryClient.Feedbacks);
            Assert.Equal("Anna Petrenko", capturedFeedback.Name);
            Assert.Equal("anna@example.com", capturedFeedback.ContactEmail);
            Assert.Equal("The settings layout needs a clearer success state.", capturedFeedback.Message);
            Assert.Equal("http://localhost/settings", capturedFeedback.Url);
            Assert.Null(capturedFeedback.AssociatedEventId);
        });
    }

    [Test]
    public void LoggingErrorBoundary_FatalCrash_OpensFeedbackDialog_WithAssociatedEventId()
    {
        var expectedEventId = SentryId.Create();
        _harness.SentryClient.NextExceptionId = expectedEventId;

        var cut = Render<LoggingErrorBoundary>(parameters => parameters
            .AddChildContent<ThrowingFeedbackComponent>());

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.FindByTestId(UiTestIds.Diagnostics.Fatal));
            Assert.NotNull(cut.FindByTestId(UiTestIds.Feedback.Dialog));

            var feedback = Services.GetRequiredService<SentryUserFeedbackService>();
            Assert.NotNull(feedback.Current);
            Assert.Equal(SentryUserFeedbackPromptKind.Fatal, feedback.Current!.Kind);
            Assert.Equal("Unhandled UI render", feedback.Current.Operation);
            Assert.Equal("Forced boundary failure.", feedback.Current.Detail);
            Assert.Equal(expectedEventId, feedback.Current.AssociatedEventId);
        });
    }

    private static RuntimeTelemetryOptions CreateTelemetryOptions() =>
        new(
            GoogleAnalyticsMeasurementId: "G-test-measurement",
            ClarityProjectId: "clarity-test-project",
            SentryDsn: "https://public@example.ingest.sentry.io/1",
            HostEnabled: true);

    private sealed class ThrowingFeedbackComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            throw new InvalidOperationException("Forced boundary failure.");
        }
    }
}
