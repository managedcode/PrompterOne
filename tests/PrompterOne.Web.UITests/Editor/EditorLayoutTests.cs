using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services.Editor;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class EditorLayoutTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_MetadataRailStaysDockedToRightOfMainPanel()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);

            var mainPanel = page.GetByTestId(UiTestIds.Editor.MainPanel);
            var metadataRail = page.GetByTestId(UiTestIds.Editor.MetadataRail);

            await Expect(mainPanel)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(metadataRail)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var mainBounds = await GetRequiredBoundingBoxAsync(mainPanel);
            var railBounds = await GetRequiredBoundingBoxAsync(metadataRail);
            var dockGap = railBounds.X - (mainBounds.X + mainBounds.Width);
            var bottomEdgeDrift = Math.Abs((railBounds.Y + railBounds.Height) - (mainBounds.Y + mainBounds.Height));

            Assert.InRange(
                Math.Abs(dockGap - BrowserTestConstants.Editor.MetadataRailDockGapPx),
                0,
                BrowserTestConstants.Editor.MetadataRailDockTolerancePx);
            Assert.InRange(
                Math.Abs(railBounds.Y - mainBounds.Y),
                0,
                BrowserTestConstants.Editor.MetadataRailDockTolerancePx);
            Assert.InRange(
                bottomEdgeDrift,
                0,
                BrowserTestConstants.Editor.MetadataRailDockTolerancePx);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_SourceEditorUsesSingleVerticalScrollSurface()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Editor);

            var sourceScrollHost = page.GetByTestId(UiTestIds.Editor.SourceScrollHost);

            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await Expect(sourceScrollHost)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            await EditorMonacoDriver.SetTextAsync(
                page,
                string.Join(
                    '\n',
                    Enumerable.Range(1, BrowserTestConstants.Editor.ScrollProbeLineCount)
                        .Select(index => $"Scroll probe line {index}")));

            await page.EvaluateAsync(
                """
                args => {
                    const harness = window[args.harnessGlobalName];
                    const state = harness?.getState(args.testId);
                    if (!state) {
                        throw new Error("Monaco scroll harness state is unavailable.");
                    }

                    const host = document.querySelector(`[data-testid="${args.testId}"]`);
                    if (!(host instanceof HTMLElement)) {
                        throw new Error("Monaco scroll host is unavailable.");
                    }

                    const editorScrollSurface = host.querySelector('.monaco-scrollable-element');
                    if (!(editorScrollSurface instanceof HTMLElement)) {
                        throw new Error("Monaco scroll surface is unavailable.");
                    }

                    editorScrollSurface.scrollTop = editorScrollSurface.scrollHeight;
                    editorScrollSurface.dispatchEvent(new Event('scroll', { bubbles: true }));
                }
                """,
                new
                {
                    harnessGlobalName = EditorMonacoRuntimeContract.BrowserHarnessGlobalName,
                    testId = UiTestIds.Editor.SourceStage
                });

            var stageState = await EditorMonacoDriver.GetStateAsync(page);
            var scrollState = await sourceScrollHost.EvaluateAsync<EditorScrollState>(
                """
                element => {
                    return {
                        hostScrollTop: element.scrollTop,
                        hostOverflowY: getComputedStyle(element).overflowY
                    };
                }
                """);

            Assert.True(stageState.ScrollTop > 0);
            Assert.Equal(BrowserTestConstants.Editor.MaxSourceScrollHostTopPx, scrollState.HostScrollTop);
            Assert.Equal("hidden", scrollState.HostOverflowY);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_CreatedDateFieldShowsVisibleCalendarIcon()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);

            var createdInput = page.GetByTestId(UiTestIds.Editor.Created);
            var createdIcon = page.GetByTestId(UiTestIds.Editor.CreatedIcon);

            await Expect(createdInput)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(createdIcon)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var iconColor = await createdIcon.EvaluateAsync<string>(
                """
                element => getComputedStyle(element).color
                """);

            Assert.NotEqual(BrowserTestConstants.Editor.CalendarIconUnexpectedColor, iconColor);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_ToolbarKeepsFarActionsReachableOnPhoneLandscape()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.SetViewportSizeAsync(
                BrowserTestConstants.ResponsiveLayout.IphoneMediumHeight,
                BrowserTestConstants.ResponsiveLayout.IphoneMediumWidth);
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);

            var toolbar = page.GetByTestId(UiTestIds.Editor.Toolbar);
            var aiButton = page.GetByTestId(UiTestIds.Editor.Ai);

            await Expect(toolbar)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(aiButton)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var toolbarState = await toolbar.EvaluateAsync<ToolbarOverflowState>(
                """
                element => ({
                    clientWidth: element.clientWidth,
                    scrollWidth: element.scrollWidth,
                    overflowX: getComputedStyle(element).overflowX
                })
                """);

            Assert.True(
                new[] { "auto", "scroll" }.Contains(toolbarState.OverflowX, StringComparer.Ordinal),
                $"Unexpected toolbar overflow-x value: {toolbarState.OverflowX}");

            await toolbar.EvaluateAsync(
                """
                element => {
                    element.scrollLeft = element.scrollWidth;
                    element.dispatchEvent(new Event('scroll', { bubbles: true }));
                }
                """);

            var toolbarBounds = await GetRequiredBoundingBoxAsync(toolbar);
            var aiButtonBounds = await GetRequiredBoundingBoxAsync(aiButton);
            var aiOverflowRight = (aiButtonBounds.X + aiButtonBounds.Width) - (toolbarBounds.X + toolbarBounds.Width);
            var aiOverflowLeft = toolbarBounds.X - aiButtonBounds.X;

            Assert.InRange(aiOverflowRight, double.MinValue, BrowserTestConstants.EditorFlow.ToolbarOverflowTolerancePx);
            Assert.InRange(aiOverflowLeft, double.MinValue, BrowserTestConstants.EditorFlow.ToolbarOverflowTolerancePx);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_UsesAvailableWidthAndMetadataRailCanCollapse()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.LayoutScenario);

            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);

            var layout = page.GetByTestId(UiTestIds.Editor.Layout);
            var mainPanel = page.GetByTestId(UiTestIds.Editor.MainPanel);
            var metadataRail = page.GetByTestId(UiTestIds.Editor.MetadataRail);
            var metadataToggle = page.GetByTestId(UiTestIds.Editor.MetadataRailToggle);

            await Expect(layout)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(mainPanel)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(metadataRail)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await Expect(metadataToggle)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var expandedMetrics = await ReadLayoutMetricsAsync(page);
            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BrowserTestConstants.EditorFlow.LayoutScenario,
                BrowserTestConstants.EditorFlow.LayoutExpandedStep);

            Assert.InRange(
                expandedMetrics.LayoutViewportRightGap,
                0,
                BrowserTestConstants.Editor.MaximumLayoutViewportRightGapPx);
            Assert.False(expandedMetrics.MetadataRailCollapsed);
            Assert.Equal(BrowserTestConstants.EditorFlow.MetadataRailExpandedChevronDirection, expandedMetrics.MetadataToggleChevronDirection);

            await metadataToggle.ClickAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.EditorFlow.MetadataRailToggleSettleDelayMs);
            await Expect(metadataToggle).ToHaveAttributeAsync("aria-expanded", "false");

            var collapsedMetrics = await ReadLayoutMetricsAsync(page);
            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BrowserTestConstants.EditorFlow.LayoutScenario,
                BrowserTestConstants.EditorFlow.LayoutCollapsedStep);

            Assert.InRange(
                collapsedMetrics.LayoutViewportRightGap,
                0,
                BrowserTestConstants.Editor.MaximumLayoutViewportRightGapPx);
            Assert.True(collapsedMetrics.MetadataRailCollapsed);
            Assert.Equal(BrowserTestConstants.EditorFlow.MetadataRailCollapsedChevronDirection, collapsedMetrics.MetadataToggleChevronDirection);
            Assert.True(
                expandedMetrics.MainWidth + BrowserTestConstants.Editor.MinimumMainPanelGrowthOnCollapsePx <= collapsedMetrics.MainWidth,
                $"Expected the main editor panel to grow by at least {BrowserTestConstants.Editor.MinimumMainPanelGrowthOnCollapsePx}px after collapsing metadata, but it changed from {expandedMetrics.MainWidth:0.##} to {collapsedMetrics.MainWidth:0.##}.");
            Assert.InRange(
                collapsedMetrics.MetadataRailWidth,
                0,
                BrowserTestConstants.Editor.MaximumCollapsedMetadataRailWidthPx);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task<LayoutBounds> GetRequiredBoundingBoxAsync(ILocator locator) =>
        await locator.EvaluateAsync<LayoutBounds>(
            """
            element => {
                const rect = element.getBoundingClientRect();
                return {
                    x: rect.x,
                    y: rect.y,
                    width: rect.width,
                    height: rect.height
                };
            }
            """);

    private static async Task<EditorLayoutMetrics> ReadLayoutMetricsAsync(IPage page) =>
        await page.EvaluateAsync<EditorLayoutMetrics>(
            """
            ({ layoutTestId, mainTestId, railTestId, toggleTestId }) => {
                const layout = document.querySelector(`[data-testid="${layoutTestId}"]`);
                const main = document.querySelector(`[data-testid="${mainTestId}"]`);
                const rail = document.querySelector(`[data-testid="${railTestId}"]`);
                const toggle = document.querySelector(`[data-testid="${toggleTestId}"]`);

                if (!(layout instanceof HTMLElement) || !(main instanceof HTMLElement) || !(rail instanceof HTMLElement) || !(toggle instanceof HTMLElement)) {
                    throw new Error("Editor layout metrics target is unavailable.");
                }

                const layoutRect = layout.getBoundingClientRect();
                const mainRect = main.getBoundingClientRect();
                const railRect = rail.getBoundingClientRect();

                return {
                    layoutViewportRightGap: window.innerWidth - layoutRect.right,
                    mainWidth: mainRect.width,
                    metadataRailWidth: railRect.width,
                    metadataRailCollapsed: rail.getAttribute('data-collapsed') === 'true',
                    metadataToggleChevronDirection: toggle.getAttribute('data-chevron-direction') ?? ''
                };
            }
            """,
            new
            {
                layoutTestId = UiTestIds.Editor.Layout,
                mainTestId = UiTestIds.Editor.MainPanel,
                railTestId = UiTestIds.Editor.MetadataRail,
                toggleTestId = UiTestIds.Editor.MetadataRailToggle
            });

    private readonly record struct ToolbarOverflowState(double ClientWidth, double ScrollWidth, string OverflowX);
    private readonly record struct EditorScrollState(double HostScrollTop, string HostOverflowY);
    private readonly record struct LayoutBounds(double X, double Y, double Width, double Height);
    private readonly record struct EditorLayoutMetrics(
        double LayoutViewportRightGap,
        double MainWidth,
        double MetadataRailWidth,
        bool MetadataRailCollapsed,
        string MetadataToggleChevronDirection);
}
