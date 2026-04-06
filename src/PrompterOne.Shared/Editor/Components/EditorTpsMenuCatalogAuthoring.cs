using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Components.Editor;

internal static partial class EditorTpsMenuCatalog
{
    private static IReadOnlyList<EditorTpsMenuGroupDefinition> CreateInsertGroupDefinitions() =>
    [
        new("Structure", [
            Insert("insert-segment-menu", "float-insert-segment-menu", UiTestIds.Editor.InsertSegmentMenu, UiTestIds.Editor.FloatingInsertSegmentMenu, EditorActionContents.Label("##", "Segment", "Speaker aware", EditorActionContentTone.Structure, mono: true), "Segment — order-independent TPS 1.1.0 header. ## [Name|Speaker:Host|140WPM|emotion|0:00-0:30]", "## [Segment Name|Speaker:Host|140WPM|neutral|0:00-0:30]\n"),
            Insert("insert-segment-archetype-menu", "float-insert-segment-archetype-menu", UiTestIds.Editor.InsertSegmentArchetypeMenu, UiTestIds.Editor.FloatingInsertSegmentArchetypeMenu, EditorActionContents.Label("##", "Segment", "Archetype aware", EditorActionContentTone.SpeedFast, mono: true), "Segment with an archetype preset. ## [Name|Speaker:Host|Archetype:Coach|emotion|0:00-0:30]", "## [Segment Name|Speaker:Host|Archetype:Coach|neutral|0:00-0:30]\n"),
            Insert("insert-block-menu", "float-insert-block-menu", UiTestIds.Editor.InsertBlockMenu, UiTestIds.Editor.FloatingInsertBlockMenu, EditorActionContents.Label("###", "Block", "Speaker aware", EditorActionContentTone.Structure, mono: true), "Block — order-independent TPS 1.1.0 header. ### [Name|Speaker:Host|140WPM|emotion]", "### [Block Name|Speaker:Host|140WPM|focused]\n"),
            Insert("insert-block-archetype-menu", "float-insert-block-archetype-menu", UiTestIds.Editor.InsertBlockArchetypeMenu, UiTestIds.Editor.FloatingInsertBlockArchetypeMenu, EditorActionContents.Label("###", "Block", "Archetype aware", EditorActionContentTone.SpeedFast, mono: true), "Block with an archetype preset. ### [Name|Speaker:Host|Archetype:Educator|emotion]", "### [Block Name|Speaker:Host|Archetype:Educator|focused]\n")
        ]),
        new("Pronunciation", [
            Wrap("insert-phonetic-menu", "float-insert-phonetic-menu", "editor-insert-phonetic-menu", "editor-float-insert-phonetic-menu", EditorActionContents.IconLabel(EditorActionIconKind.Phonetic, "Phonetic (IPA)", "[phonetic:...]", EditorActionIconTone.Purple), "IPA phonetic notation for precise pronunciation. [phonetic:IPA]word[/phonetic]", "[phonetic:IPA]", "[/phonetic]", "word"),
            Wrap("insert-pronunciation", "float-insert-pronunciation", "editor-insert-pronunciation", UiTestIds.Editor.FloatingInsertPronunciation, EditorActionContents.Label("Aa", "Pronunciation", "[pronunciation:...]", EditorActionContentTone.Pronunciation), "Simple pronunciation guide for easy reading. [pronunciation:guide]word[/pronunciation]", "[pronunciation:guide]", "[/pronunciation]", "word"),
            Wrap("insert-stress-guide", "float-insert-stress-guide", "editor-insert-stress-guide", "editor-float-insert-stress-guide", EditorActionContents.Label("S", "Stress guide", "[stress:...]", EditorActionContentTone.Stress), "Stress cue with an explicit guide. [stress:rising]word[/stress]", "[stress:rising]", "[/stress]", "word")
        ], HasSeparatorBefore: true),
        new("Edit Points", [
            Insert("insert-edit-point-high", "float-insert-edit-point-high", "editor-insert-edit-point-high", "editor-float-insert-edit-point-high", EditorActionContents.IconLabel(EditorActionIconKind.EditPoint, "High priority", "[edit_point:high]", EditorActionIconTone.Red), "High priority edit point — critical, must review. [edit_point:high]", "[edit_point:high]"),
            Insert("insert-edit-point-medium", "float-insert-edit-point-medium", "editor-insert-edit-point-medium", UiTestIds.Editor.FloatingInsertEditPointMedium, EditorActionContents.IconLabel(EditorActionIconKind.EditPoint, "Medium priority", "[edit_point:medium]", EditorActionIconTone.Orange), "Medium priority edit point — important but not critical. [edit_point:medium]", "[edit_point:medium]"),
            Insert("insert-edit-point-standard", "float-insert-edit-point-standard", "editor-insert-edit-point-standard", "editor-float-insert-edit-point-standard", EditorActionContents.IconLabel(EditorActionIconKind.EditPoint, "Standard", "[edit_point]", EditorActionIconTone.Gold), "Standard edit point — marks a natural break. [edit_point]", "[edit_point]")
        ], HasSeparatorBefore: true)
    ];

    private static IReadOnlyList<EditorTpsMenuGroupDefinition> CreatePauseGroupDefinitions() =>
    [
        new("Breath And Pauses", [
            Insert("pause-breath", "float-pause-breath-menu", "editor-pause-breath", "editor-float-pause-breath-menu", EditorActionContents.IconLabel(EditorActionIconKind.PauseBars, "Breath", "[breath]"), "Breath mark between phrases. [breath]", "[breath]"),
            Insert("pause-short-menu", "float-pause-short-menu", "editor-pause-short-menu", "editor-float-pause-short-menu", EditorActionContents.Label("/", "Short pause", "300ms", EditorActionContentTone.Structure), "Short 300ms pause between clauses. Inserted as / in text", "/"),
            Insert("pause-medium-menu", "float-pause-medium-menu", "editor-pause-medium-menu", "editor-float-pause-medium-menu", EditorActionContents.Label("//", "Medium pause", "600ms", EditorActionContentTone.Structure), "Medium 600ms pause between sentences. Inserted as // in text", "//"),
            Insert("pause-1s", "float-pause-1s", "editor-pause-one-second", "editor-float-pause-1s", EditorActionContents.IconLabel(EditorActionIconKind.PauseClock, "1 second", "[pause:1s]"), "1 second timed pause. [pause:1s]", "[pause:1s]"),
            Insert("pause-2s", "float-pause-2s", "editor-pause-two-seconds", "editor-float-pause-2s", EditorActionContents.IconLabel(EditorActionIconKind.PauseClock, "2 seconds", "[pause:2s]"), "2 second timed pause. [pause:2s]", "[pause:2s]"),
            Insert("pause-3s", "float-pause-3s", "editor-pause-three-seconds", "editor-float-pause-3s", EditorActionContents.IconLabel(EditorActionIconKind.PauseClock, "3 seconds", "[pause:3s]"), "3 second timed pause. [pause:3s]", "[pause:3s]"),
            Insert("pause-custom", "float-pause-custom", "editor-pause-custom", UiTestIds.Editor.FloatingPauseTimed, EditorActionContents.IconLabel(EditorActionIconKind.PauseClock, "Custom (ms)", "[pause:Nms]"), "Custom timed pause in milliseconds. [pause:500ms]", "[pause:500ms]")
        ])
    ];

    private static IReadOnlyList<EditorTpsMenuGroupDefinition> CreateSpeedGroupDefinitions() =>
    [
        new("Speed Presets", [
            Wrap("speed-xslow-menu", "float-speed-xslow-menu", "editor-speed-xslow-menu", "editor-float-speed-xslow-menu", EditorActionContents.Label("×0.6", "Extra Slow", "[xslow]", EditorActionContentTone.SpeedXslow, mono: true), "Very careful delivery. Critical warnings. base × 0.6 = 84 WPM at base 140", "[xslow]", "[/xslow]"),
            Wrap("speed-slow-menu", "float-speed-slow-menu", "editor-speed-slow-menu", "editor-float-speed-slow-menu", EditorActionContents.Label("×0.8", "Slow", "[slow]", EditorActionContentTone.SpeedSlow, mono: true), "Important points, emphasis. base × 0.8 = 112 WPM at base 140", "[slow]", "[/slow]"),
            Wrap("speed-fast-menu", "float-speed-fast-menu", "editor-speed-fast-menu", "editor-float-speed-fast-menu", EditorActionContents.Label("×1.25", "Fast", "[fast]", EditorActionContentTone.SpeedFast, mono: true), "Quick mentions, asides. base × 1.25 = 175 WPM at base 140", "[fast]", "[/fast]"),
            Wrap("speed-xfast-menu", "float-speed-xfast-menu", "editor-speed-xfast-menu", "editor-float-speed-xfast-menu", EditorActionContents.Label("×1.5", "Extra Fast", "[xfast]", EditorActionContentTone.SpeedXfast, mono: true), "Rapid transitions, low-importance text. base × 1.5 = 210 WPM at base 140", "[xfast]", "[/xfast]"),
            Wrap("speed-normal-menu", "float-speed-normal-menu", "editor-speed-normal-menu", "editor-float-speed-normal-menu", EditorActionContents.Label("×1.0", "Normal", "[normal]", EditorActionContentTone.SpeedNormal, mono: true), "Reset to base speed. base × 1.0 = 140 WPM at base 140", "[normal]", "[/normal]")
        ]),
        new("Custom Speed", [
            Wrap("speed-custom-wpm", "float-speed-custom-wpm", "editor-speed-custom-wpm", UiTestIds.Editor.FloatingSpeedCustomWpm, EditorActionContents.IconLabel(EditorActionIconKind.Lightning, "Custom WPM", "[NWPM]"), "Set an absolute WPM value for a text span. [180WPM]text[/180WPM]", "[180WPM]", "[/180WPM]")
        ], HasSeparatorBefore: true)
    ];
}
