using PrompterOne.Shared.Components.Editor;

namespace PrompterOne.Web.Tests;

public sealed class EditorToolbarCatalogContractTests
{
    [Fact]
    public void TopToolbarDropdowns_FollowVendoredTpsOrder()
    {
        var emotionGroup = FindTopDropdownGroup("emotion", "TPS Emotions");
        var deliveryGroup = FindTopDropdownGroup("emotion", "Delivery Modes");
        var speedGroup = FindTopDropdownGroup("speed", "Speed Presets");

        Assert.Equal(
            [
                "emotion-neutral",
                "emotion-warm",
                "emotion-professional",
                "emotion-focused",
                "emotion-concerned",
                "emotion-urgent",
                "emotion-motivational",
                "emotion-excited",
                "emotion-happy",
                "emotion-sad",
                "emotion-calm",
                "emotion-energetic"
            ],
            emotionGroup.Actions.Select(action => action.Key));

        Assert.Equal(
            ["delivery-sarcasm", "delivery-aside", "delivery-rhetorical", "delivery-building"],
            deliveryGroup.Actions.Select(action => action.Key));

        Assert.Equal(
            ["speed-xslow-menu", "speed-slow-menu", "speed-fast-menu", "speed-xfast-menu", "speed-normal-menu"],
            speedGroup.Actions.Select(action => action.Key));
    }

    [Fact]
    public void FloatingToolbarDropdowns_FollowVendoredTpsOrder()
    {
        var emotionGroup = FindFloatingDropdownGroup(EditorToolbarMenuIds.FloatingEmotion, "TPS Emotions");
        var deliveryGroup = FindFloatingDropdownGroup(EditorToolbarMenuIds.FloatingEmotion, "Delivery Modes");
        var speedGroup = FindFloatingDropdownGroup(EditorToolbarMenuIds.FloatingSpeed, "Speed Presets");

        Assert.Equal(
            [
                "float-emotion-neutral",
                "float-emotion-warm",
                "float-emotion-professional",
                "float-emotion-focused",
                "float-emotion-concerned",
                "float-emotion-urgent",
                "float-emotion-motivational",
                "float-emotion-excited",
                "float-emotion-happy",
                "float-emotion-sad",
                "float-emotion-calm",
                "float-emotion-energetic"
            ],
            emotionGroup.Actions.Select(action => action.Key));

        Assert.Equal(
            ["float-delivery-sarcasm", "float-delivery-aside", "float-delivery-rhetorical", "float-delivery-building"],
            deliveryGroup.Actions.Select(action => action.Key));

        Assert.Equal(
            ["float-speed-xslow-menu", "float-speed-slow-menu", "float-speed-fast-menu", "float-speed-xfast-menu", "float-speed-normal-menu"],
            speedGroup.Actions.Select(action => action.Key));
    }

    private static EditorToolbarDropdownGroupDescriptor FindTopDropdownGroup(string sectionKey, string groupLabel)
    {
        var section = Assert.Single(EditorToolbarCatalog.Sections, candidate => string.Equals(candidate.Key, sectionKey, StringComparison.Ordinal));
        return Assert.Single(section.DropdownGroups, candidate => string.Equals(candidate.Label, groupLabel, StringComparison.Ordinal));
    }

    private static EditorToolbarDropdownGroupDescriptor FindFloatingDropdownGroup(string menuId, string groupLabel)
    {
        var menu = Assert.Single(EditorToolbarCatalog.FloatingMenus, candidate => string.Equals(candidate.MenuId, menuId, StringComparison.Ordinal));
        return Assert.Single(menu.DropdownGroups, candidate => string.Equals(candidate.Label, groupLabel, StringComparison.Ordinal));
    }
}
