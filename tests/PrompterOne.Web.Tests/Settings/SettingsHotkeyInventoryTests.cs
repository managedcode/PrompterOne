using Bunit;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Tests;

namespace PrompterOne.Web.Tests;

public sealed class SettingsHotkeyInventoryTests : BunitContext
{
    public static IEnumerable<AppHotkeyGroup> HotkeyGroups => AppHotkeys.Groups;

    public static IEnumerable<(string GroupId, string DefinitionId)> HotkeyActions =>
        AppHotkeys.Groups.SelectMany(
            group => group.Definitions.Select(definition => (group.Id, definition.Id)));

    [Test]
    [MethodDataSource(nameof(HotkeyGroups))]
    public void SettingsPage_ShortcutsSection_RendersCatalogGroup(AppHotkeyGroup group)
    {
        TestHarnessFactory.Create(this);

        var cut = Render<SettingsPage>();

        cut.FindByTestId(UiTestIds.Settings.NavShortcuts).Click();

        cut.WaitForAssertion(() =>
        {
            _ = cut.FindByTestId(UiTestIds.Settings.ShortcutsPanel);
            cut.FindByTestId(UiTestIds.Settings.ShortcutsGroup(group.Id));
        });
    }

    [Test]
    [MethodDataSource(nameof(HotkeyActions))]
    public void SettingsPage_ShortcutsSection_RendersCatalogAction(string groupId, string definitionId)
    {
        TestHarnessFactory.Create(this);

        var cut = Render<SettingsPage>();

        cut.FindByTestId(UiTestIds.Settings.NavShortcuts).Click();

        cut.WaitForAssertion(() =>
        {
            _ = cut.FindByTestId(UiTestIds.Settings.ShortcutsPanel);
            cut.FindByTestId(UiTestIds.Settings.ShortcutsAction(groupId, definitionId));
        });
    }

    [Test]
    public void SettingsPage_ShortcutsSection_DoesNotMentionObs()
    {
        TestHarnessFactory.Create(this);

        var cut = Render<SettingsPage>();

        cut.FindByTestId(UiTestIds.Settings.NavShortcuts).Click();

        cut.WaitForAssertion(() =>
        {
            var panel = cut.FindByTestId(UiTestIds.Settings.ShortcutsPanel);
            Assert.DoesNotContain("OBS", panel.TextContent, StringComparison.OrdinalIgnoreCase);
        });
    }
}
