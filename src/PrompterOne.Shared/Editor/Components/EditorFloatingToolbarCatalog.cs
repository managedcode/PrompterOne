using PrompterOne.Core.Models.Editor;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Components.Editor;

public sealed record EditorFloatingMenuDescriptor(
    string MenuId,
    string TriggerTestId,
    string PanelTestId,
    string Label,
    IReadOnlyList<EditorToolbarDropdownGroupDescriptor> DropdownGroups,
    string? Style = null);

internal static class EditorFloatingToolbarCatalog
{
    private const string ChevronDownIcon = """<svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="6,9 12,15 18,9" /></svg>""";
    private const string SparkIcon = """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L9 9 2 12l7 3 3 7 3-7 7-3-7-3z" /></svg>""";
    private const string PauseClockIcon = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10" /><polyline points="12,6 12,12 16,14" /></svg>""";
    private const string PauseFloatIcon = """<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="4" width="4" height="16"></rect><rect x="14" y="4" width="4" height="16"></rect></svg>""";
    private const string LightningIcon = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="13,2 3,14 12,14 11,22 21,10 12,10" /></svg>""";
    private const string EditPointIconGold = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#FFD060" stroke-width="2"><path d="M12 2L2 7l10 5 10-5-10-5z" /><path d="M2 17l10 5 10-5" /><path d="M2 12l10 5 10-5" /></svg>""";
    private const string EditPointIconRed = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#FF6060" stroke-width="2" style="flex-shrink:0"><path d="M12 2L2 7l10 5 10-5-10-5z" /><path d="M2 17l10 5 10-5" /><path d="M2 12l10 5 10-5" /></svg>""";
    private const string EditPointIconOrange = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#FFB86A" stroke-width="2" style="flex-shrink:0"><path d="M12 2L2 7l10 5 10-5-10-5z" /><path d="M2 17l10 5 10-5" /><path d="M2 12l10 5 10-5" /></svg>""";
    private const string HighlightIcon = """<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#FFE066" stroke-width="2"><rect x="3" y="14" width="18" height="6" rx="2"></rect><path d="M8 14V8a4 4 0 0 1 8 0v6"></path></svg>""";
    private const string PhoneticIcon = """<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#D88AFF" stroke-width="2"><path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z" /><path d="M19 10v2a7 7 0 0 1-14 0v-2" /></svg>""";

    public static IReadOnlyList<IReadOnlyList<EditorToolbarActionDescriptor>> ActionGroups { get; } =
    [
        [
            EditorFloatingToolbarActionFactory.Wrap("float-bold", "<b>B</b>", "Bold — strong emphasis level 2. Markdown: **text**", "editor-float-bold", "**", "**"),
            EditorFloatingToolbarActionFactory.Wrap("float-italic", "<i>I</i>", "Italic — emphasis level 1. Markdown: *text*", "editor-float-italic", "*", "*"),
            EditorFloatingToolbarActionFactory.Wrap("float-emphasis", "Em", "Emphasis [emphasis]", UiTestIds.Editor.FloatEmphasis, "[emphasis]", "[/emphasis]"),
            EditorFloatingToolbarActionFactory.Wrap("float-highlight", HighlightIcon, "Highlight [highlight]", "editor-float-highlight", "[highlight]", "[/highlight]"),
            EditorFloatingToolbarActionFactory.Wrap("float-stress", "<span style=\"color:#FFD060;font-weight:700\">S</span>", "Stress [stress]", UiTestIds.Editor.FloatStress, "[stress]", "[/stress]")
        ],
        [
            EditorFloatingToolbarActionFactory.Wrap("float-voice-loud", "<span class=\"cdot\" style=\"background:#FFD060\"></span>", "Loud [loud]", "editor-float-voice-loud", "[loud]", "[/loud]"),
            EditorFloatingToolbarActionFactory.Toggle(EditorToolbarMenuIds.FloatingVoice, "<span class=\"cdot\" style=\"background:#FFD060\"></span>" + ChevronDownIcon, "Voice cues", UiTestIds.Editor.FloatingVoice),
            EditorFloatingToolbarActionFactory.Toggle(EditorToolbarMenuIds.FloatingEmotion, "<span class=\"cdot\" style=\"background:#FFB840\"></span>" + ChevronDownIcon, "Emotion and delivery", UiTestIds.Editor.FloatingEmotion)
        ],
        [
            EditorFloatingToolbarActionFactory.Wrap("float-slow", ".8×", "Slow [slow]", UiTestIds.Editor.FloatingSlow, "[slow]", "[/slow]"),
            EditorFloatingToolbarActionFactory.Wrap("float-fast", "1.25×", "Fast [fast]", "editor-float-fast", "[fast]", "[/fast]"),
            EditorFloatingToolbarActionFactory.Insert("float-pause", PauseFloatIcon, "Breath [breath]", UiTestIds.Editor.FloatingPause, "[breath]"),
            EditorFloatingToolbarActionFactory.Toggle(EditorToolbarMenuIds.FloatingPause, PauseClockIcon + ChevronDownIcon, "Pause cues", UiTestIds.Editor.FloatingPauseTrigger),
            EditorFloatingToolbarActionFactory.Toggle(EditorToolbarMenuIds.FloatingSpeed, LightningIcon + ChevronDownIcon, "Speed cues", UiTestIds.Editor.FloatingSpeedTrigger)
        ],
        [
            EditorFloatingToolbarActionFactory.Toggle(EditorToolbarMenuIds.FloatingInsert, EditPointIconGold + ChevronDownIcon, "Insert TPS helpers", UiTestIds.Editor.FloatingInsert),
            EditorFloatingToolbarActionFactory.Ai("float-ai", $"{SparkIcon}AI", "AI — rewrite, expand, simplify", UiTestIds.Editor.FloatingAi, EditorAiAssistAction.Simplify)
        ]
    ];

    public static IReadOnlyList<EditorFloatingMenuDescriptor> Menus { get; } =
    [
        new(
            EditorToolbarMenuIds.FloatingVoice,
            UiTestIds.Editor.FloatingVoice,
            UiTestIds.Editor.FloatingVoiceMenu,
            "Voice Cues",
            [
                new("Volume", [
                    EditorFloatingToolbarActionFactory.Wrap("float-voice-loud-menu", "<span style=\"color:#FFB86A;font-weight:700;width:44px;text-align:right\">LOUD</span> Loud <small>[loud]</small>", "Raise vocal force without changing emotion. [loud]text[/loud]", "editor-float-voice-loud-menu", "[loud]", "[/loud]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-voice-soft-menu", "<span style=\"color:#B7D9FF;font-weight:700;width:44px;text-align:right\">SOFT</span> Soft <small>[soft]</small>", "Soften delivery and keep the phrase gentle. [soft]text[/soft]", "editor-float-voice-soft-menu", "[soft]", "[/soft]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-voice-whisper-menu", "<span style=\"color:#CFCBD7;font-weight:700;width:44px;text-align:right\">LOW</span> Whisper <small>[whisper]</small>", "Very quiet, intimate delivery. [whisper]text[/whisper]", UiTestIds.Editor.FloatingVoiceWhisper, "[whisper]", "[/whisper]", cssClass: "efb-menu-item")
                ]),
                new("Stress", [
                    EditorFloatingToolbarActionFactory.Wrap("float-voice-stress-menu", "<span style=\"color:#FFD060;font-weight:700;width:44px;text-align:right\">MARK</span> Stress <small>[stress]</small>", "Stress a word or phrase inline. [stress]text[/stress]", "editor-float-voice-stress-menu", "[stress]", "[/stress]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-voice-stress-guide-menu", "<span style=\"color:#FFD060;font-weight:700;width:44px;text-align:right\">GUIDE</span> Stress guide <small>[stress:...]</small>", "Add a spoken stress cue. [stress:rising]text[/stress]", "editor-float-voice-stress-guide-menu", "[stress:rising]", "[/stress]", placeholder: "text", cssClass: "efb-menu-item")
                ], HasSeparatorBefore: true),
                new("Reset", [
                    EditorFloatingToolbarActionFactory.ClearColor("float-voice-clear", "Remove supported inline TPS wrappers from the selected text", "editor-float-voice-clear")
                ], HasSeparatorBefore: true)
            ]),
        new(
            EditorToolbarMenuIds.FloatingEmotion,
            UiTestIds.Editor.FloatingEmotion,
            UiTestIds.Editor.FloatingEmotionMenu,
            "Emotion",
            [
                new("TPS Emotions", [
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-warm", "<span class=\"cdot\" style=\"background:#FFB840\"></span> Warm <small>😊</small>", "Friendly, welcoming tone. Inline: [warm]text[/warm]", "editor-float-emotion-warm", "[warm]", "[/warm]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-concerned", "<span class=\"cdot\" style=\"background:#FF7A7A\"></span> Concerned <small>😟</small>", "Worried, empathetic. Inline: [concerned]text[/concerned]", "editor-float-emotion-concerned", "[concerned]", "[/concerned]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-focused", "<span class=\"cdot\" style=\"background:#4AE0A0\"></span> Focused <small>🎯</small>", "Concentrated, precise. Inline: [focused]text[/focused]", "editor-float-emotion-focused", "[focused]", "[/focused]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-motivational", "<span class=\"cdot\" style=\"background:#C88AFF\"></span> Motivational <small>💪</small>", "Inspiring, encouraging. Inline: [motivational]text[/motivational]", UiTestIds.Editor.FloatingEmotionMotivational, "[motivational]", "[/motivational]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-neutral", "<span class=\"cdot\" style=\"background:#8ECFFF\"></span> Neutral <small>😐</small>", "Default balanced tone. Inline: [neutral]text[/neutral]", "editor-float-emotion-neutral", "[neutral]", "[/neutral]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-urgent", "<span class=\"cdot\" style=\"background:#FF6060\"></span> Urgent <small>🚨</small>", "Critical, immediate attention. Inline: [urgent]text[/urgent]", "editor-float-emotion-urgent", "[urgent]", "[/urgent]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-happy", "<span class=\"cdot\" style=\"background:#FFE87A\"></span> Happy <small>😄</small>", "Joyful, positive. Inline: [happy]text[/happy]", "editor-float-emotion-happy", "[happy]", "[/happy]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-excited", "<span class=\"cdot\" style=\"background:#FF8AC8\"></span> Excited <small>🚀</small>", "Enthusiastic, energetic. Inline: [excited]text[/excited]", "editor-float-emotion-excited", "[excited]", "[/excited]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-sad", "<span class=\"cdot\" style=\"background:#A0A8FF\"></span> Sad <small>😢</small>", "Melancholy, somber. Inline: [sad]text[/sad]", "editor-float-emotion-sad", "[sad]", "[/sad]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-calm", "<span class=\"cdot\" style=\"background:#5EECC2\"></span> Calm <small>😌</small>", "Peaceful, relaxed. Inline: [calm]text[/calm]", "editor-float-emotion-calm", "[calm]", "[/calm]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-energetic", "<span class=\"cdot\" style=\"background:#FFA050\"></span> Energetic <small>⚡</small>", "High energy, dynamic. Inline: [energetic]text[/energetic]", "editor-float-emotion-energetic", "[energetic]", "[/energetic]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-emotion-professional", "<span class=\"cdot\" style=\"background:#80B8FF\"></span> Professional <small>💼</small>", "Business-like, formal. Inline: [professional]text[/professional]", UiTestIds.Editor.FloatingEmotionProfessional, "[professional]", "[/professional]", cssClass: "efb-menu-item")
                ]),
                new("Delivery Modes", [
                    EditorFloatingToolbarActionFactory.Wrap("float-delivery-aside", "<span style=\"color:#B8C0C8;font-weight:700;width:44px;text-align:right\">ASIDE</span> Aside <small>[aside]</small>", "Mark the phrase as an aside. [aside]text[/aside]", "editor-float-delivery-aside", "[aside]", "[/aside]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-delivery-rhetorical", "<span style=\"color:#FFE87A;font-weight:700;width:44px;text-align:right\">WHY?</span> Rhetorical <small>[rhetorical]</small>", "Use rhetorical delivery without changing emotion. [rhetorical]text[/rhetorical]", "editor-float-delivery-rhetorical", "[rhetorical]", "[/rhetorical]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-delivery-sarcasm", "<span style=\"color:#F19A9A;font-weight:700;width:44px;text-align:right\">SARC</span> Sarcasm <small>[sarcasm]</small>", "Apply a sarcastic delivery cue. [sarcasm]text[/sarcasm]", UiTestIds.Editor.FloatingDeliverySarcasm, "[sarcasm]", "[/sarcasm]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-delivery-building", "<span style=\"color:#D2B8E3;font-weight:700;width:44px;text-align:right\">BUILD</span> Building <small>[building]</small>", "Gradually build intensity through the phrase. [building]text[/building]", "editor-float-delivery-building", "[building]", "[/building]", cssClass: "efb-menu-item")
                ], HasSeparatorBefore: true)
            ]),
        new(
            EditorToolbarMenuIds.FloatingPause,
            UiTestIds.Editor.FloatingPauseTrigger,
            UiTestIds.Editor.FloatingPauseMenu,
            "Pause",
            [
                new("Breath And Pauses", [
                    EditorFloatingToolbarActionFactory.Insert("float-pause-breath-menu", $"{PauseFloatIcon} Breath <small>[breath]</small>", "Breath mark between phrases. [breath]", "editor-float-pause-breath-menu", "[breath]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-short-menu", "<span style=\"color:var(--gold-text);font-weight:700;width:20px;text-align:center\">/</span> Short pause <small>300ms</small>", "Short 300ms pause between clauses. Inserted as / in text", "editor-float-pause-short-menu", "/", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-medium-menu", "<span style=\"color:var(--gold-text);font-weight:700;width:20px;text-align:center\">//</span> Medium pause <small>600ms</small>", "Medium 600ms pause between sentences. Inserted as // in text", "editor-float-pause-medium-menu", "//", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-1s", $"{PauseClockIcon} 1 second <small>[pause:1s]</small>", "1 second timed pause. [pause:1s]", "editor-float-pause-1s", "[pause:1s]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-2s", $"{PauseClockIcon} 2 seconds <small>[pause:2s]</small>", "2 second timed pause. [pause:2s]", "editor-float-pause-2s", "[pause:2s]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-3s", $"{PauseClockIcon} 3 seconds <small>[pause:3s]</small>", "3 second timed pause. [pause:3s]", "editor-float-pause-3s", "[pause:3s]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-pause-custom", $"{PauseClockIcon} 1000ms <small>[pause:1000ms]</small>", "Custom timed pause in milliseconds. [pause:1000ms]", UiTestIds.Editor.FloatingPauseTimed, "[pause:1000ms]", cssClass: "efb-menu-item")
                ])
            ]),
        new(
            EditorToolbarMenuIds.FloatingSpeed,
            UiTestIds.Editor.FloatingSpeedTrigger,
            UiTestIds.Editor.FloatingSpeedMenu,
            "Speed",
            [
                new("Speed Presets", [
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-xslow-menu", "<span style=\"color:#FF8A8A;font-family:var(--font-mono);font-size:11px;font-weight:700;width:36px;text-align:right\">×0.6</span> Extra Slow <small>[xslow]</small>", "Very careful delivery. Critical warnings. base × 0.6 = 84 WPM at base 140", "editor-float-speed-xslow-menu", "[xslow]", "[/xslow]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-slow-menu", "<span style=\"color:#FFB86A;font-family:var(--font-mono);font-size:11px;font-weight:700;width:36px;text-align:right\">×0.8</span> Slow <small>[slow]</small>", "Important points, emphasis. base × 0.8 = 112 WPM at base 140", "editor-float-speed-slow-menu", "[slow]", "[/slow]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-normal-menu", "<span style=\"color:var(--text-2);font-family:var(--font-mono);font-size:11px;font-weight:700;width:36px;text-align:right\">×1.0</span> Normal <small>[normal]</small>", "Reset to base speed. base × 1.0 = 140 WPM at base 140", "editor-float-speed-normal-menu", "[normal]", "[/normal]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-fast-menu", "<span style=\"color:#8ECFFF;font-family:var(--font-mono);font-size:11px;font-weight:700;width:36px;text-align:right\">×1.25</span> Fast <small>[fast]</small>", "Quick mentions, asides. base × 1.25 = 175 WPM at base 140", "editor-float-speed-fast-menu", "[fast]", "[/fast]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-xfast-menu", "<span style=\"color:#80B8FF;font-family:var(--font-mono);font-size:11px;font-weight:700;width:36px;text-align:right\">×1.5</span> Extra Fast <small>[xfast]</small>", "Rapid transitions, low-importance text. base × 1.5 = 210 WPM at base 140", "editor-float-speed-xfast-menu", "[xfast]", "[/xfast]", cssClass: "efb-menu-item")
                ]),
                new("Custom Speed", [
                    EditorFloatingToolbarActionFactory.Wrap("float-speed-custom-wpm", $"{LightningIcon} Custom WPM <small>[NWPM]</small>", "Set an absolute WPM value for a text span. [180WPM]text[/180WPM]", UiTestIds.Editor.FloatingSpeedCustomWpm, "[180WPM]", "[/180WPM]", cssClass: "efb-menu-item")
                ], HasSeparatorBefore: true)
            ],
            "min-width:260px"),
        new(
            EditorToolbarMenuIds.FloatingInsert,
            UiTestIds.Editor.FloatingInsert,
            UiTestIds.Editor.FloatingInsertMenu,
            "Insert",
            [
                new("Edit Points", [
                    EditorFloatingToolbarActionFactory.Insert("float-insert-edit-point-high", $"{EditPointIconRed} High priority <small>[edit_point:high]</small>", "High priority edit point — critical, must review. [edit_point:high]", "editor-float-insert-edit-point-high", "[edit_point:high]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-insert-edit-point-medium", $"{EditPointIconOrange} Medium priority <small>[edit_point:medium]</small>", "Medium priority edit point — important but not critical. [edit_point:medium]", UiTestIds.Editor.FloatingInsertEditPointMedium, "[edit_point:medium]", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Insert("float-insert-edit-point-standard", $"{EditPointIconGold} Standard <small>[edit_point]</small>", "Standard edit point — marks a natural break. [edit_point]", "editor-float-insert-edit-point-standard", "[edit_point]", cssClass: "efb-menu-item")
                ]),
                new("Pronunciation", [
                    EditorFloatingToolbarActionFactory.Wrap("float-insert-phonetic-menu", $"{PhoneticIcon} Phonetic (IPA) <small>[phonetic:...]</small>", "IPA phonetic notation for precise pronunciation. [phonetic:IPA]word[/phonetic]", "editor-float-insert-phonetic-menu", "[phonetic:IPA]", "[/phonetic]", placeholder: "word", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-insert-pronunciation", "<span style=\"color:#D88AFF;font-weight:600;width:14px;text-align:center;flex-shrink:0;font-size:13px\">Aa</span> Pronunciation <small>[pronunciation:...]</small>", "Simple pronunciation guide for easy reading. [pronunciation:guide]word[/pronunciation]", UiTestIds.Editor.FloatingInsertPronunciation, "[pronunciation:guide]", "[/pronunciation]", placeholder: "word", cssClass: "efb-menu-item"),
                    EditorFloatingToolbarActionFactory.Wrap("float-insert-stress-guide", "<span style=\"color:#FFD060;font-weight:700;width:14px;text-align:center;flex-shrink:0;font-size:13px\">S</span> Stress guide <small>[stress:...]</small>", "Stress cue with an explicit guide. [stress:rising]word[/stress]", "editor-float-insert-stress-guide", "[stress:rising]", "[/stress]", placeholder: "word", cssClass: "efb-menu-item")
                ], HasSeparatorBefore: true)
            ],
            "min-width:260px")
    ];

    public static EditorFloatingMenuDescriptor? FindMenu(string? menuId) =>
        Menus.FirstOrDefault(menu => string.Equals(menu.MenuId, menuId, StringComparison.Ordinal));
}
