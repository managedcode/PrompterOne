# TPS Reference

## Intent

`PrompterOne` implements the current TPS 1.1.x contract from the upstream [managedcode/TPS README](https://github.com/managedcode/TPS/blob/main/README.md). This repo-local reference replaces the deleted root TPS prototype document and records the product-facing TPS rules that the app, tests, and docs must keep aligned.
## Implementation Anchors

- Vendored upstream TPS SDK: `src/PrompterOne.TpsSdk/`
- Source-to-document mapping: `src/PrompterOne.Core/Tps/Services/TpsDocumentReader.cs`
- Source-to-ScriptData mapping: `src/PrompterOne.Core/Tps/Services/TpsScriptDataFactory.cs`
- Compilation: `src/PrompterOne.Core/Tps/Services/ScriptCompiler.cs`
- Library-card metrics: `src/PrompterOne.Shared/Library/Services/LibraryCardFactory.cs`
- Editor TPS authoring menus: `src/PrompterOne.Shared/Editor/Components/EditorToolbarCatalog.cs`
- Monaco TPS language support: `src/PrompterOne.Shared/wwwroot/editor/editor-monaco-tps-language-spec.js`
# TPS Format Specification (TelePrompterScript)

## What is TPS?

TPS (TelePrompterScript) is a markdown-based file format for teleprompter scripts. It supports hierarchical content organization with precise timing, emotional cues, delivery instructions, and presentation control.

## Motivation

Teleprompter scripts need more than plain text. A speaker needs pace guidance, emotional cues, visual emphasis, and natural pauses — all embedded directly in the script. Existing formats (plain text, SubRip `.srt`, WebVTT) either lack structure entirely or focus on subtitle timing rather than live presentation flow.

TPS bridges this gap: it is human-readable markdown that any text editor can open, yet it carries rich metadata that teleprompter software can interpret for real-time rendering.

## Target Audience

- **Script authors** — writers who create teleprompter scripts and want inline pace/emotion control without leaving a text editor.
- **Teleprompter app developers** — engineers who parse and render TPS files in real-time reading applications.
- **Content producers** — video/podcast teams who need structured, reviewable scripts with edit points and timing hints.

## Design Goals

1. **Markdown-compatible** — a `.tps` file renders reasonably in any markdown viewer.
2. **Human-authorable** — no tooling required to write a valid script.
3. **Hierarchical** — documents decompose into Segments → Blocks → Phrases → Words, each level inheriting and overriding properties.
4. **Minimal yet extensible** — the core tag set is small; parsers ignore unknown tags gracefully.

## Glossary

Complete canonical glossary, including `#`, `##`, `###`, inline tags, diagnostics, compiler/runtime terms, and SDK terminology: [docs/Glossary.md](https://github.com/managedcode/TPS/blob/main/docs/Glossary.md)

| Term | Definition |
|------|-----------|
| **Script** | The entire TPS document, including front matter and all content. |
| **Front Matter** | YAML metadata block delimited by `---` at the top of the file. |
| **Title** | The optional `#` heading used as display metadata for the script. |
| **Segment** | A major section (`##` header) — e.g., Intro, Problem, Solution. |
| **Block** | A topic group (`###` header) within a segment. |
| **Phrase** | A sentence or thought within a block, delimited by sentence-ending punctuation or pause markers. |
| **Word** | An individual token with optional per-word properties (emphasis, volume, pause). |
| **WPM** | Words Per Minute — the reading speed. |
| **Edit Point** | A marker indicating a natural place to stop or start an editing session. |
| **Emotion** | A predefined delivery style that controls tone, energy, and visual presentation. |
| **Articulation** | How words connect during delivery: `legato` (smooth, flowing) or `staccato` (sharp, separated). |
| **Energy** | Overall intensity and dynamism of delivery on a 1–10 scale. |
| **Melody** | Pitch variation and inflection contour on a 1–10 scale. |
| **Vocal Archetype** | A composite delivery persona (`Friend`, `Motivator`, `Educator`, `Coach`, `Storyteller`, `Entertainer`) that defines expected ranges for all delivery parameters. |
| **Validator** | The TPS component that reports actionable authoring diagnostics. |
| **Compiler** | The TPS component that turns parsed TPS into a JSON-friendly timed state machine. |
| **Compiled Script** | The runtime-ready output containing metadata, segments, blocks, phrases, words, and timing. |
| **Player** | The runtime component that resolves what should be shown at a specific elapsed time. |
| **ManagedCode.Tps SDK** | The multi-runtime SDK workspace under `SDK/` for TypeScript, JavaScript, .NET, Flutter, Swift, and Java runtimes. |

## File Structure

```
Script
├── Front Matter (YAML metadata)
├── Title (# header — optional, informational only)
└── Segments (## headers — major sections)
    └── Blocks (### headers — topic groups within segments)
        └── Phrases (sentences/thoughts with inline markers)
            └── Words (with individual properties)
```

### Title (`#` Level)

An optional `#` header immediately after the front matter. It serves as the document title for display purposes. Parsers should treat it as metadata — it does not create a segment or affect WPM/emotion inheritance. If omitted, the `title` field from the front matter is used instead. If both are present, the `#` header takes precedence for display.

## Format Specification

### Front Matter (YAML)

The front matter is a YAML block delimited by `---` lines at the very start of the file. All fields are optional.

```yaml
---
title: "Script Title"
profile: Actor              # Actor (default)
duration: "10:00"           # Target duration MM:SS
base_wpm: 140               # Default words per minute
speed_offsets:              # Percentage offsets for speed tags relative to base_wpm
  xslow: -40                # [xslow] = base_wpm × 0.6 (40% slower)
  slow: -20                 # [slow] = base_wpm × 0.8 (20% slower)
  fast: 25                  # [fast] = base_wpm × 1.25 (25% faster)
  xfast: 50                 # [xfast] = base_wpm × 1.5 (50% faster)
author: "Author Name"
created: "2024-01-01"
version: "1.0"
---
```

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `title` | string | — | Human-readable script title. |
| `profile` | string | `Actor` | `Actor` for natural spoken reading from a teleprompter. |
| `duration` | string | — | Target duration in `MM:SS` format. |
| `base_wpm` | integer | 140 | Default reading speed (words per minute) for the entire document. |
| `speed_offsets.xslow` | integer | -40 | Percentage offset for `[xslow]...[/xslow]` tags. Default -40 means 60% of base speed. |
| `speed_offsets.slow` | integer | -20 | Percentage offset for `[slow]...[/slow]` tags. Default -20 means 80% of base speed. |
| `speed_offsets.fast` | integer | 25 | Percentage offset for `[fast]...[/fast]` tags. Default 25 means 125% of base speed. |
| `speed_offsets.xfast` | integer | 50 | Percentage offset for `[xfast]...[/xfast]` tags. Default 50 means 150% of base speed. |
| `author` | string | — | Script author name. |
| `created` | string | — | Creation date (ISO 8601). |
| `version` | string | — | Document version. |

### Segments (`##` Level)

Segments are major sections of the script.

```markdown
## [SegmentName|120WPM|Emotion|Timing|Archetype:Name]
```

All parameters after the name are optional, separated by `|`. Parameters are identified by format, not by position:

- An integer (or integer + `WPM` suffix) → **WPM**
- A known emotion keyword → **Emotion**
- A time pattern (`MM:SS` or `MM:SS-MM:SS`) → **Timing**
- `Speaker:Name` → **Speaker** (for multi-talent scripts)
- `Archetype:Name` → **Vocal Archetype** (delivery persona with validation)

This means parameters can appear in any order and unneeded ones can simply be omitted — no empty `||` slots required.

**Examples:**
- `## [Intro|Warm]` — inherits base WPM, sets emotion
- `## [Urgent Update|145WPM|Urgent|0:30-1:10]` — overrides WPM, sets emotion and timing
- `## [Overview|Neutral]` — inherits WPM, sets emotion
- `## [Simple Segment]` — name only, inherits everything
- `## [Opening|Warm|Archetype:Friend]` — sets emotion and vocal archetype
- `## [Rally|Motivational|Archetype:Motivator|Speaker:Alex]` — archetype with speaker

**Segment Parameters:**

| Parameter | Format | Description |
|-----------|--------|-------------|
| **Name** | free text | Human-readable label shown in editors. Required (first value before the first `\|`). |
| **WPM** | `NNN` or `NNNWPM` | Integer speed override. Omit to inherit. |
| **Emotion** | preset name | Emotion preset (see table below). Omit to inherit (defaults to `Neutral` at document level). |
| **Timing** | `MM:SS` or `MM:SS-MM:SS` | Duration hint. Stored for tooling; playback computes timing from word counts. |
| **Speaker** | `Speaker:Name` | Talent assignment for multi-speaker scripts. Omit for single-speaker. |
| **Archetype** | `Archetype:Name` | Vocal archetype for delivery validation. See [Vocal Archetypes](#vocal-archetypes). |

**Leading text:** Content between a segment header and its first block is preserved as introductory text that inherits the segment's speed and emotion.

### Blocks (`###` Level)

Blocks are topic groups within a segment.

```markdown
### [BlockName|120WPM|Emotion|Archetype:Name]
```

**Examples:**
- `### [Opening Block]` — inherits segment WPM, emotion, and archetype
- `### [Speed Variations|Focused]` — inherits WPM, overrides emotion
- `### [Key Stats|130WPM]` — overrides WPM, inherits emotion
- `### [Climax|150WPM|Urgent]` — overrides both
- `### [Drill|Archetype:Coach]` — overrides archetype, inherits everything else

**Block Parameters:**

| Parameter | Format | Description |
|-----------|--------|-------------|
| **Name** | free text | Descriptive label. Required (first value before the first `\|`). |
| **WPM** | `NNN` or `NNNWPM` | Integer speed override. Inherits segment WPM if omitted. |
| **Emotion** | preset name | Emotion override. Inherits segment emotion if omitted. |
| **Speaker** | `Speaker:Name` | Talent assignment. Inherits segment speaker if omitted. |
| **Archetype** | `Archetype:Name` | Vocal archetype override. Inherits segment archetype if omitted. |

### Inline Markers

Inline markers are embedded within phrase text to control presentation.

#### Pauses

```markdown
[pause:2s]          # 2-second pause
[pause:1000ms]      # 1000-millisecond pause
/                   # Short pause (300ms)
//                  # Medium pause (600ms)
```

#### Emphasis

```markdown
[emphasis]word[/emphasis]    # Standard emphasis (level 1)
*word*                       # Markdown italic → emphasis level 1
**word**                     # Markdown bold → emphasis level 2
```

#### Speed Changes

```markdown
[150WPM]text here[/150WPM]    # Temporary absolute speed change
[xslow]very careful[/xslow]   # Relative: base_wpm × 0.6 (40% slower than base)
[slow]important point[/slow]   # Relative: base_wpm × 0.8 (20% slower than base)
[fast]quick mention[/fast]     # Relative: base_wpm × 1.25 (25% faster than base)
[xfast]rapid section[/xfast]   # Relative: base_wpm × 1.5 (50% faster than base)
[normal]reset speed[/normal]   # Reset to base_wpm (multiplier = 1.0)
```

**Speed tag summary (at base_wpm = 140):**

| Tag | Multiplier | Effective WPM |
|-----|-----------|---------------|
| `[xslow]` | 0.6× | 84 |
| `[slow]` | 0.8× | 112 |
| `[normal]` | 1.0× | 140 |
| `[fast]` | 1.25× | 175 |
| `[xfast]` | 1.5× | 210 |

**Note:** All relative speed tags are **relative to the base speed**, not absolute values. The multiplier is calculated as `1 + (offset / 100)`. For example, `slow` with offset `-20` → multiplier `1 + (-20/100)` = `0.8`. Tags stack multiplicatively when nested: `[xslow][slow]text[/slow][/xslow]` = base × 0.6 × 0.8 = 48% of base.

**Note on `[xfast]`:** At the default base of 140 WPM, `[xfast]` produces 210 WPM — above the comfortable teleprompter range (100–170). This is intentional: `[xfast]` is designed for short throwaway phrases where the teleprompter scrolls quickly past low-importance text. The reader is not expected to articulate every word at 210 WPM — the visual speed cue signals "skim through this." For sustained reading, authors should prefer `[fast]` (175 WPM) or lower the `base_wpm`.

#### Runtime Speed Control

The **+/−** buttons on the reading page change the **base speed** for the current run. All relative speed tags (`[xslow]`, `[slow]`, `[fast]`, `[xfast]`) automatically scale with the new base, preserving their proportional relationship. For example, pressing **+** to increase from 140 to 150 WPM means `[slow]` changes from 112 to 120 WPM.

#### Volume

```markdown
[loud]important announcement[/loud]       # Louder delivery
[soft]gentle aside[/soft]                 # Softer, quieter delivery
[whisper]secret or intimate[/whisper]     # Whispered delivery
```

Volume tags control the **intended loudness** of delivery. Renderers should visually distinguish volume levels (e.g., larger/bolder text for `[loud]`, smaller/lighter for `[soft]`, styled differently for `[whisper]`).

#### Highlighting

```markdown
[highlight]key point[/highlight]    # Visual highlighting (background overlay)
```

`highlight` is a visual formatting tag — it applies a semi-transparent background overlay to make text stand out on the teleprompter. It does not imply a specific delivery change.

#### Emotion Styling

```markdown
[warm]friendly greeting[/warm]      # Apply emotion-based styling inline
[urgent]breaking news[/urgent]      # Inline emotion override
```

#### Breath Marks

```markdown
[breath]                            # Natural breath point
```

A breath mark indicates where the speaker should take a breath. Unlike pauses, breath marks do not add time — they guide the reader to breathe naturally at that point. Useful in long passages where phrasing might otherwise cause the reader to run out of air.

#### Edit Points

```markdown
[edit_point:high]                   # High priority edit point
[edit_point:medium]                 # Medium priority edit point
[edit_point]                        # Standard edit point (no priority)
```

#### Pronunciation Guides

```markdown
[phonetic:ˈkæməl]camel[/phonetic]              # IPA pronunciation
[pronunciation:KAM-uhl]camel[/pronunciation]    # Simple guide
```

#### Syllable Stress

**Inline wrap** — wrap the stressed part of a word:

```markdown
develop[stress]me[/stress]nt        # Stress on "me"
[stress]in[/stress]frastructure     # Stress on "in"
```

The tag can wrap a single letter or a syllable. Renderers should visually distinguish the stressed portion (e.g., larger font size, underline, or bold).

**Stress guide** — full syllable breakdown with a parameter:

```markdown
[stress:de-VE-lop-ment]development[/stress]
[stress:IN-fra-struc-ture]infrastructure[/stress]
```

Hyphens separate syllables. The stressed syllable is **UPPERCASE**, unstressed are lowercase. Renderers should display the guide as a tooltip or overlay — not replace the word. Use this for complex or unfamiliar words where the reader needs the full pronunciation map.

#### Delivery Mode Tags

```markdown
[sarcasm]Oh, that went really well[/sarcasm]         # Say the opposite of what you mean
[aside]By the way, we also support webhooks[/aside]  # Parenthetical, "to the audience"
[rhetorical]Isn't that exactly what we need?[/rhetorical]  # Question delivered as statement
[building]Each phrase gets bigger. And bigger. And BIGGER.[/building]  # Gradually rising energy
```

Delivery mode tags describe **how** to deliver a passage, beyond emotion and volume:

| Tag | Delivery | Use when |
|-----|----------|----------|
| `[sarcasm]` | Say it straight, but mean the opposite. Deadpan or exaggerated. | Comedy, skepticism, irony. |
| `[aside]` | Step out of the main narrative. Lower energy, more intimate. | Parenthetical comments, tangents, notes to audience. |
| `[rhetorical]` | Deliver as a statement with question syntax. No rising intonation. | Rhetorical questions that don't expect answers. |
| `[building]` | Start at lower energy, gradually increase through the passage. | Reveals, motivational build-ups, crescendo moments. |

#### Articulation

```markdown
[legato]smooth connected delivery[/legato]     # Words flow together, melodic
[staccato]sharp. punchy. direct.[/staccato]    # Crisp, separated, clipped
```

Articulation controls how words **connect to each other** during delivery — the shape between words, not within them:

| Tag | Delivery | Think | Use when |
|-----|----------|-------|----------|
| `[legato]` | Smooth, connected flow. Words blend into each other with minimal gaps. Melodic, conversational rhythm. | Best friend chatting, motivational speaker building momentum. | Storytelling, emotional connection, motivational passages, friendly tone. |
| `[staccato]` | Sharp, separated, punchy. Each word lands distinctly with a crisp gap. Direct and percussive. | Football coach giving commands, news anchor on breaking news. | Instructions, coaching, urgency, authority, key takeaways. |

Articulation is independent of speed — you can be slow-and-staccato (deliberate, hammering each word) or fast-and-legato (rapid but flowing). It stacks with all other tags:

```markdown
[staccato][loud]Stop. Think. Act.[/loud][/staccato]       # Coach style
[legato][soft]Let that idea settle in...[/soft][/legato]   # Warm storytelling
[staccato][fast]Check. Confirm. Deploy.[/fast][/staccato]  # Rapid-fire checklist
```

#### Energy Level

```markdown
[energy:8]Let's make this happen![/energy]     # High energy (scale 1–10)
[energy:3]Now, consider this quietly...[/energy]  # Low energy
```

Energy level sets the **overall intensity and dynamism** of delivery on a 1–10 scale. It is different from volume (how loud) and speed (how fast) — energy is the combination of physicality, vocal projection, facial engagement, and body language intensity.

| Level | Description | Think |
|-------|-------------|-------|
| 1–2 | Minimal energy. Still, meditative, almost monotone. | Guided meditation, quiet reflection. |
| 3–4 | Low energy. Calm, measured, composed. Minimal movement. | University lecturer explaining a concept. |
| 5–6 | Moderate energy. Engaged but controlled. Natural conversation level. | Podcast host, casual presenter. |
| 7–8 | High energy. Dynamic, animated, strong body language. Projected voice. | Motivational speaker, football coach. |
| 9–10 | Maximum energy. Explosive, commanding, full physicality. | Championship halftime speech, rally cry. |

Energy stacks naturally with other delivery tags to create distinct archetypes:

```markdown
## [Rally Cry|Motivational]

[legato][energy:9]**You** have worked for this moment.
Every early morning, every late night — it all comes down to right now.[/energy][/legato]

## [Game Plan|Focused]

[staccato][energy:8]Three plays. **Run** the ball. **Control** the clock. **Win** the game.[/staccato]

## [Lecture|Professional]

[energy:3]The second law of thermodynamics states that entropy
in an isolated system never decreases. //
[energy:4]Now, why does this matter for our model?
```

#### Melody (Pitch Variation)

```markdown
[melody:8]What an incredible journey this has been![/melody]   # Melodic, expressive
[melody:2]The system processes 14 requests per second.[/melody]  # Flat, matter-of-fact
```

Melody controls the **pitch variation and inflection contour** of delivery on a 1–10 scale. Low melody = monotone, informational. High melody = musical, expressive, wide pitch swings.

| Level | Description | Think |
|-------|-------------|-------|
| 1–2 | Flat, monotone. Minimal pitch variation. Matter-of-fact. | Reading a spec, reciting data, deadpan delivery. |
| 3–4 | Low melody. Subtle inflection, controlled. Informational but not robotic. | University lecturer, news anchor, documentary narrator. |
| 5–6 | Moderate melody. Natural conversational inflection. | Podcast host, casual presenter, everyday speech. |
| 7–8 | High melody. Expressive, musical quality. Wide pitch range. | Storyteller, motivational speaker building momentum. |
| 9–10 | Dramatic melody. Theatrical, sweeping pitch contours. Voice paints pictures. | Voice actor in a dramatic narration, preacher in full flow. |

Melody is independent of speed and energy — a slow, low-energy passage can still be highly melodic (think: a lullaby), and a fast, high-energy passage can be flat (think: an auctioneer).

#### Speaker Tags

For multi-talent scripts, mark who reads each section:

```markdown
## [Interview|Warm|Speaker:Alex]

### [Question|Speaker:Jordan]
So tell us about the project. //

### [Answer|Speaker:Alex]
We started with a simple idea. //
```

The `Speaker:Name` parameter is added to segment or block headers using the same `|`-separated syntax. The parser identifies it by the `Speaker:` prefix. Renderers should visually distinguish speakers (e.g., different text colors or labels).

#### Vocal Archetypes

```markdown
## [Opening|Warm|Archetype:Friend]

[legato][energy:5][melody:7]Hey, thanks for being here today.
I really appreciate you taking the time. //
So, let me tell you what we've been working on...[/melody][/energy][/legato]

## [The Why|Motivational|Archetype:Motivator]

[legato][energy:9][melody:8]**You** have the power to change this.
Every single one of you / has what it takes.[/melody][/energy][/legato]

## [The What|Professional|Archetype:Educator]

[energy:3][melody:3]The framework consists of three components. //
First, the ingestion layer. //
Second, the processing pipeline. //
Third, the output adapter.[/melody][/energy]

## [Action Items|Focused|Archetype:Coach]

[staccato][energy:8][melody:2][loud]Three things. **Today.** //
One — ship the fix. //
Two — update the docs. //
Three — notify the team.[/loud][/melody][/energy][/staccato]
```

A **vocal archetype** defines the overall delivery persona for a segment or block.

TPS draws this concept from two related speaking frameworks:

1. [Vinh Giang](https://www.vinhgiang.com/) frames delivery as a switch between distinct **communicative intents**. In practice that means moving between modes such as connecting, inspiring, informing, or instructing. His core set includes four archetypes: Friend, Motivator, Educator, and Coach. See his explanations on [LinkedIn](https://www.linkedin.com/posts/vinhgiang_what-are-vocal-archetypes-activity-7095585018655223808-4rnD) and [Facebook](https://www.facebook.com/askvinh/videos/vocal-archetypes/404161770260317/).
2. [Roger Love](https://rogerlove.com/) describes the measurable mechanics underneath vocal delivery: pitch, pace, tone, melody, and volume. TPS maps archetypes onto those controls so they can be expressed with inline tags and playback metadata. See [How to Use Your Voice More Effectively](https://rogerlove.com/how-to-use-your-voice-more-effectively/) for the underlying framework.

TPS extends Giang's original set with two additional delivery personas used often in broadcast and voiceover coaching:

- Storyteller
- Entertainer

In TPS syntax, the archetype is declared with `Archetype:Name` inside a segment or block header using the same `|`-separated parameter format. The parser recognizes it by the `Archetype:` prefix.

##### Archetype Definitions

TPS defines **six** vocal archetypes. Each archetype has an expected range for every delivery parameter. These ranges power **archetype validation** — when an archetype is set, the parser can warn if inline tags conflict with the archetype's expected delivery profile.

| Archetype | Goal | Exemplar | Think |
|-----------|------|----------|-------|
| `Friend` | Connect | Brene Brown | Best friend over coffee |
| `Motivator` | Inspire | Tony Robbins | Motivational speaker at a rally |
| `Educator` | Inform | Neil deGrasse Tyson | University lecturer |
| `Coach` | Guide & instruct | Football coach | Halftime team talk |
| `Storyteller` | Transport & engage | Barack Obama | Campfire narrator |
| `Entertainer` | Delight | Jerry Seinfeld | Stand-up comedian |

##### Archetype Parameter Profiles

Each archetype has expected delivery parameter ranges. These are the "ideal" values — not hard constraints, but the basis for validation warnings. WPM ranges are calibrated for **teleprompter reading** — where the speaker reads from a screen while maintaining eye contact with the camera. This is significantly slower than free speaking or memorized delivery. When an archetype is set on a segment or block **without** an explicit WPM override, the compiler uses the archetype's **recommended WPM** as the target speed for that scope.

**Friend** — _Connect_ (recommended WPM: **135**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | `legato` | Smooth, connected, conversational |
| Energy | 4–6 | Warm and relaxed, not intense |
| Melody | 6–8 | High — lots of natural inflection, musical |
| Volume | `soft` or default | Intimate, not projecting |
| Speed | 125–150 WPM | Unhurried, conversational pace |
| Pauses | Natural, comfortable | Not dramatic |
| Fillers | Tolerated | "um," "you know" add authenticity _(author guidance only — not validated)_ |

**Motivator** — _Inspire_ (recommended WPM: **155**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | `legato` | Flowing, building momentum, crescendo |
| Energy | 7–10 | Very high, passionate, expansive |
| Melody | 7–9 | Sweeping, emotional rises, dramatic |
| Volume | `loud` | Projected, fills the room |
| Speed | 145–170 WPM | Energetic, building — fastest teleprompter archetype |
| Pauses | Strategic dramatic pauses | After climactic statements — let it land |
| Fillers | None | Momentum should not be broken _(author guidance only — not validated)_ |

**Educator** — _Inform_ (recommended WPM: **120**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | none expected | Clear, precise diction — neither `[legato]` nor `[staccato]` expected; both produce warnings |
| Energy | 3–5 | Calm, grounded, steady |
| Melody | 2–4 | Low — matter-of-fact, informational |
| Volume | default | Medium, controlled, consistent |
| Speed | 110–135 WPM | Deliberate, measured — slowest archetype for comprehension |
| Pauses | Frequent, structured | Between concepts for processing |
| Fillers | None | Precision matters _(author guidance only — not validated)_ |

**Coach** — _Guide & instruct_ (recommended WPM: **145**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | `staccato` | Short, punchy, percussive delivery |
| Energy | 7–9 | High, focused, intense but controlled |
| Melody | 1–3 | Low — flat and direct, almost rhythmic |
| Volume | `loud` | Assertive, commanding |
| Speed | 135–160 WPM | Punchy, variable — fast bursts then pause |
| Pauses | Sharp, short | Between directives (beat. beat. beat.) |
| Fillers | Zero tolerance | Every word is deliberate _(author guidance only — not validated)_ |

**Storyteller** — _Transport & engage_ (recommended WPM: **125**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | both valid | Fluid with dramatic punctuation — `[legato]` and `[staccato]` both accepted, no warnings |
| Energy | 4–7 | Medium, modulated — rises and falls with narrative |
| Melody | 8–10 | Very high — the most melodic archetype |
| Volume | variable | Whisper to full projection within one story |
| Speed | 100–150 WPM | Highly variable — slows for drama, speeds for action |
| Pauses | Long, dramatic | Cliffhanger beats, suspense gaps |
| Fillers | None | Every word chosen for narrative effect _(author guidance only — not validated)_ |

**Entertainer** — _Delight_ (recommended WPM: **150**)

| Parameter | Expected | Notes |
|-----------|----------|-------|
| Articulation | both valid | `staccato` for punchlines, `legato` for setups — both accepted, no warnings |
| Energy | 6–8 | High, playful, light |
| Melody | 7–9 | High — sing-song quality, playful rises |
| Volume | variable | Medium-high with sudden drops for comedic effect |
| Speed | 140–165 WPM | Rhythmic, comedic timing is paramount |
| Pauses | Critical | Before/after punchlines |
| Fillers | Intentional | Used for comedic timing _(author guidance only — not validated)_ |

##### Archetype Validation

When a segment or block has an `Archetype:` parameter, parsers **should** validate that inline delivery tags within the scope are consistent with the archetype's expected profile. Mismatches produce **warnings** (not errors) — the author may intentionally break an archetype for effect.

Validation rules:

| Condition | Severity | Example |
|-----------|----------|---------|
| `[staccato]` inside `Archetype:Friend` | warning | Friend expects legato delivery |
| `[legato]` inside `Archetype:Coach` | warning | Coach expects staccato delivery |
| `[legato]` or `[staccato]` inside `Archetype:Educator` | warning | Educator expects no articulation tags (natural diction) |
| `[energy:2]` inside `Archetype:Motivator` | warning | Motivator expects energy 7–10 |
| `[energy:9]` inside `Archetype:Educator` | warning | Educator expects energy 3–5 |
| `[energy:3]` inside `Archetype:Entertainer` | warning | Entertainer expects energy 6–8 |
| `[melody:9]` inside `Archetype:Coach` | warning | Coach expects melody 1–3 |
| `[melody:2]` inside `Archetype:Storyteller` | warning | Storyteller expects melody 8–10 |
| `[melody:2]` inside `Archetype:Entertainer` | warning | Entertainer expects melody 7–9 |
| `[whisper]` inside `Archetype:Motivator` | warning | Motivator expects loud delivery |
| `[loud]` inside `Archetype:Friend` | warning | Friend expects soft/default volume |
| `[xfast]` inside `Archetype:Educator` | warning | Educator expects slower pace (110–135) |
| `[xslow]` inside `Archetype:Motivator` | warning | Motivator expects faster pace (145–170) |

Note: `Archetype:Storyteller` and `Archetype:Entertainer` accept both `[legato]` and `[staccato]` without warnings — both articulation styles are valid for these archetypes.

Validation is **advisory** — it helps authors stay within an archetype's established delivery profile, but deliberate violations are valid. For example, a Coach might intentionally drop to `[energy:3]` for a quiet, intense moment before building back up.

##### Archetype Rhythm Profiles

Beyond inline tag validation, each archetype has a distinct **rhythm** — measurable structural patterns in how phrases, pauses, and emphasis are distributed. Parsers **should** validate rhythm after compilation and emit warnings when the content's rhythm deviates significantly from the archetype's expected profile.

**Rhythm parameters:**

| Parameter | How measured | Description |
|-----------|-------------|-------------|
| **Phrase length** | Words per phrase (avg) | How long sentences/thoughts are before a pause or period. |
| **Pause frequency** | Pauses per 100 words | How often the speaker stops — includes `/`, `//`, `[pause:...]`. |
| **Pause duration** | Average pause ms | Short snappy pauses vs. long dramatic ones. |
| **Emphasis density** | % of words with emphasis | How many words are stressed/highlighted/bolded. |
| **Speed variation** | Count of inline speed tags per 100 words | How often speed changes within the scope. |

**Expected rhythm by archetype:**

| Archetype | Phrase length | Pause frequency | Avg pause ms | Emphasis density | Speed variation |
|-----------|--------------|-----------------|-------------|-----------------|-----------------|
| **Friend** | 8–15 words | 4–8 per 100w | 300–600 ms | Low (3–8%) | Very low (0–1) |
| **Motivator** | 8–20 words | 3–6 per 100w | 600–2000 ms | High (10–20%) | Low (0–2) |
| **Educator** | 10–25 words | 6–12 per 100w | 400–800 ms | Low (3–8%) | Low (0–2) |
| **Coach** | 3–8 words | 8–15 per 100w | 200–400 ms | Very high (15–30%) | Low (0–2) |
| **Storyteller** | 5–20 words | 4–10 per 100w | 500–3000 ms | Medium (5–12%) | High (3–6) |
| **Entertainer** | 5–15 words | 5–10 per 100w | 300–2000 ms | Medium (5–15%) | Medium (2–4) |

**Rhythm validation rules:**

| Condition | Severity | Example |
|-----------|----------|---------|
| Average phrase length > 8 words in `Archetype:Coach` | warning | Coach expects short, punchy phrases (3–8 words) |
| Average phrase length < 10 words in `Archetype:Educator` | warning | Educator expects longer, explanatory phrases (10–25 words) |
| Pause frequency < 6 per 100 words in `Archetype:Educator` | warning | Educator expects frequent pauses between concepts (6–12) |
| Pause frequency < 8 per 100 words in `Archetype:Coach` | warning | Coach expects pauses between each directive (8–15) |
| Emphasis density < 15% in `Archetype:Coach` | warning | Coach expects heavy emphasis (15–30%) |
| Emphasis density > 8% in `Archetype:Educator` | warning | Educator expects restrained emphasis (3–8%) |
| Average pause > 400 ms in `Archetype:Coach` | warning | Coach expects short, sharp pauses (200–400 ms) |
| Average pause < 500 ms in `Archetype:Storyteller` | warning | Storyteller expects longer dramatic pauses (500–3000 ms) |
| No speed variation in `Archetype:Storyteller` | warning | Storyteller expects dynamic speed changes for narrative effect |

**Example — Coach rhythm (good):**

```markdown
### [Drill|Archetype:Coach]

[staccato][energy:8][melody:2][loud]
**Ship** the fix. //
**Update** the docs. //
**Notify** the team. //
No excuses. / No delays. / **Today.**
[/loud][/melody][/energy][/staccato]
```

Rhythm analysis: avg phrase 3.3 words, 15 pauses/100w, avg pause 350ms, emphasis 33% — matches Coach profile.

**Example — Coach rhythm (warning):**

```markdown
### [Instructions|Archetype:Coach]

So what I'd like everyone to do over the next few days is to take a careful look
at the deployment pipeline and identify any areas where we might be able to
improve our overall throughput and reliability metrics. //
```

Rhythm analysis: avg phrase 35 words, 1 pause/100w, emphasis 0% — **multiple warnings**: phrases too long, not enough pauses, no emphasis. This reads like Educator, not Coach.

##### Recommended Archetype Sequencing

Vinh Giang recommends transitioning between archetypes every 3–5 minutes for maximum audience engagement. A recommended presentation flow:

```markdown
## [Opening|Warm|Archetype:Friend]
## [The Why|Motivational|Archetype:Motivator]
## [The What|Professional|Archetype:Educator]
## [Action Items|Focused|Archetype:Coach]
## [Closing|Warm|Archetype:Friend]
```

This sequence — **Friend → Motivator → Educator → Coach → Friend** — builds connection, establishes purpose, delivers content, drives action, and closes with rapport.

## Keyword Reference

### Emotions (case-insensitive)

Emotions are a **closed set** — parsers must reject unknown emotion keywords. Each emotion describes a **delivery style** that affects how the speaker reads the text: tone of voice, energy level, and pacing feel.

| Keyword | Delivery | Typical pacing | Use when |
|---------|----------|----------------|----------|
| `neutral` | Even, balanced tone. No particular emotional coloring. | Steady | Default. Informational content, transitions. |
| `warm` | Friendly, approachable. Slight smile in the voice. | Relaxed | Greetings, introductions, audience connection. |
| `professional` | Formal, authoritative. Clear articulation. | Measured | Business content, reports, official statements. |
| `focused` | Concentrated, precise. Each word matters. | Deliberate | Technical details, step-by-step instructions. |
| `concerned` | Worried, empathetic. Lower energy, careful tone. | Slower | Problems, risks, bad news, sensitive topics. |
| `urgent` | High alert, immediate attention. Tense, direct. | Faster | Breaking news, critical warnings, deadlines. |
| `motivational` | Inspiring, encouraging. Building energy. | Building | Calls to action, closing statements, rallying. |
| `excited` | Enthusiastic, high energy. Wider pitch range. | Faster | Announcements, reveals, good news. |
| `happy` | Joyful, positive. Light and upbeat. | Relaxed | Celebrations, positive results, thank-yous. |
| `sad` | Somber, reflective. Lower pitch, slower. | Slower | Loss, disappointment, memorial. |
| `calm` | Peaceful, reassuring. Steady and even. | Slow | De-escalation, meditation, closing thoughts. |
| `energetic` | Dynamic, high tempo. Punchy delivery. | Fast | Demos, action sequences, rapid-fire content. |

Renderers map each emotion to a visual style (colors, background, text treatment) appropriate for the rendering context. The exact visual representation is implementation-defined, but should be consistent and distinguishable.

### Volume Levels

| Keyword | Description |
|---------|-------------|
| `loud` | Louder, more projected delivery |
| `soft` | Quieter, gentler delivery |
| `whisper` | Whispered, intimate delivery |

### Delivery Modes

Delivery modes are a **closed set** — parsers should treat unknown delivery mode keywords as unknown tags (see Error Handling).

| Keyword | Delivery | Use when |
|---------|----------|----------|
| `sarcasm` | Say it straight, but mean the opposite. Deadpan or exaggerated. | Comedy, skepticism, irony. |
| `aside` | Step out of the main narrative. Lower energy, more intimate. | Parenthetical comments, tangents, notes to audience. |
| `rhetorical` | Deliver as a statement with question syntax. No rising intonation. | Rhetorical questions that don't expect answers. |
| `building` | Start at lower energy, gradually increase through the passage. | Reveals, motivational build-ups, crescendo moments. |

### Articulation Styles

Articulation styles are a **closed set** — parsers should treat unknown articulation keywords as unknown tags.

| Keyword | Delivery | Use when |
|---------|----------|----------|
| `legato` | Smooth, connected. Words blend together with minimal gaps. Melodic flow. | Storytelling, emotional connection, conversational tone, motivational passages. |
| `staccato` | Sharp, separated, punchy. Each word lands distinctly with a crisp gap. | Instructions, coaching cues, authority, key takeaways, rapid-fire lists. |

### Energy Levels

Energy is specified with the `[energy:N]` tag where N is an integer from **1 to 10**. Parsers must reject values outside this range.

| Range | Description | Think |
|-------|-------------|-------|
| 1–2 | Minimal. Still, meditative, near-monotone. | Guided meditation, quiet reflection. |
| 3–4 | Low. Calm, measured, minimal movement. | University lecturer, thoughtful narrator. |
| 5–6 | Moderate. Engaged but controlled. | Podcast host, casual presenter. |
| 7–8 | High. Dynamic, animated, projected. | Motivational speaker, football coach. |
| 9–10 | Maximum. Explosive, commanding, full physicality. | Championship speech, rally cry. |

### Melody Levels

Melody is specified with the `[melody:N]` tag where N is an integer from **1 to 10**. Parsers must reject values outside this range.

| Range | Description | Think |
|-------|-------------|-------|
| 1–2 | Flat, monotone. Minimal pitch variation. | Reading a spec, reciting data, deadpan. |
| 3–4 | Low melody. Subtle inflection, controlled. | News anchor, documentary narrator. |
| 5–6 | Moderate. Natural conversational inflection. | Podcast host, casual speech. |
| 7–8 | High melody. Expressive, musical, wide pitch range. | Storyteller, motivational build-up. |
| 9–10 | Dramatic. Theatrical, sweeping pitch contours. | Voice actor, preacher in full flow. |

### Vocal Archetype Keywords

Vocal archetypes are a **closed set** — parsers should treat unknown archetype keywords as invalid header parameters. Each archetype defines a composite delivery persona based on Vinh Giang's vocal communication framework.

The `Archetype:Name` parameter is added to segment or block headers using the `|`-separated syntax. The parser identifies it by the `Archetype:` prefix.

| Keyword | Goal | Recommended WPM | Articulation | Energy | Melody |
|---------|------|-----------------|--------------|--------|--------|
| `Friend` | Connect | 135 | `legato` | 4–6 | 6–8 |
| `Motivator` | Inspire | 155 | `legato` | 7–10 | 7–9 |
| `Educator` | Inform | 120 | none expected | 3–5 | 2–4 |
| `Coach` | Guide & instruct | 145 | `staccato` | 7–9 | 1–3 |
| `Storyteller` | Transport & engage | 125 | both valid | 4–7 | 8–10 |
| `Entertainer` | Delight | 150 | both valid | 6–8 | 7–9 |

See the [Vocal Archetypes](#vocal-archetypes) section for full parameter profiles and validation rules.

### Inline Tags

| Tag | Syntax | Description |
|-----|--------|-------------|
| **Pause** | `[pause:Ns]` or `[pause:Nms]` | Pause for N seconds/milliseconds |
| **Short pause** | `/` | 300ms pause |
| **Medium pause** | `//` | 600ms pause |
| **Emphasis** | `[emphasis]text[/emphasis]` | Standard emphasis (level 1) |
| **Markdown emphasis** | `*text*` | Converted to emphasis level 1 |
| **Strong emphasis** | `**text**` | Converted to emphasis level 2 |
| **Highlight** | `[highlight]text[/highlight]` | Visual highlighting |
| **Speed (absolute)** | `[NWPM]text[/NWPM]` | Temporary absolute speed change (e.g., `[150WPM]`) |
| **Speed (preset)** | `[xslow]text[/xslow]` | Extra slow: base × 0.6 |
| **Speed (preset)** | `[slow]text[/slow]` | Slow: base × 0.8 |
| **Speed (preset)** | `[fast]text[/fast]` | Fast: base × 1.25 |
| **Speed (preset)** | `[xfast]text[/xfast]` | Extra fast: base × 1.5 |
| **Speed (reset)** | `[normal]text[/normal]` | Reset to base speed: base × 1.0 |
| **Edit point** | `[edit_point]` or `[edit_point:priority]` | Mark edit location |
| **Loud** | `[loud]text[/loud]` | Louder, projected delivery |
| **Soft** | `[soft]text[/soft]` | Quieter, gentler delivery |
| **Whisper** | `[whisper]text[/whisper]` | Whispered delivery |
| **Phonetic** | `[phonetic:IPA]text[/phonetic]` | IPA pronunciation guide |
| **Pronunciation** | `[pronunciation:guide]text[/pronunciation]` | Simple pronunciation guide |
| **Stress (wrap)** | `develop[stress]me[/stress]nt` | Stressed part of a word |
| **Stress (guide)** | `[stress:de-VE-lop-ment]word[/stress]` | Full syllable breakdown |
| **Breath** | `[breath]` | Natural breath point (no added time) |
| **Sarcasm** | `[sarcasm]text[/sarcasm]` | Deadpan/ironic delivery |
| **Aside** | `[aside]text[/aside]` | Parenthetical, "to the audience" |
| **Rhetorical** | `[rhetorical]text[/rhetorical]` | Question delivered as statement |
| **Building** | `[building]text[/building]` | Gradually rising energy/crescendo |
| **Legato** | `[legato]text[/legato]` | Smooth, connected flow between words |
| **Staccato** | `[staccato]text[/staccato]` | Sharp, punchy, separated delivery |
| **Energy** | `[energy:N]text[/energy]` | Energy/intensity level (1–10 scale) |
| **Melody** | `[melody:N]text[/melody]` | Pitch variation/inflection (1–10 scale) |
| **Emotion** | `[warm]text[/warm]`, `[urgent]...[/urgent]`, etc. | Inline emotion override (see Emotions table) |

### Speed Presets

All speed presets are **relative to base_wpm** — they apply a multiplier, not an absolute value.

| Preset | Multiplier | Effect at base 140 | Usage |
|--------|-----------|---------------------|-------|
| `xslow` | 0.6× | 84 WPM | Very careful delivery, critical warnings |
| `slow` | 0.8× | 112 WPM | Important points, emphasis |
| `fast` | 1.25× | 175 WPM | Quick mentions, asides |
| `xfast` | 1.5× | 210 WPM | Rapid transitions, low-importance text |
| `normal` | 1.0× | 140 WPM | Reset to base speed |

### Edit Point Priorities

| Priority | Usage |
|----------|-------|
| `high` | Critical edit points, must review |
| `medium` | Important but not critical |
| (none) | Standard edit point |

### Escape Sequences

| Sequence | Result | Usage |
|----------|--------|-------|
| `\[` | `[` | Literal bracket in text |
| `\]` | `]` | Literal bracket in text |
| `\|` | `|` | Literal pipe in segment/block names |
| `\/` | `/` | Literal slash (not a pause) |
| `\*` | `*` | Literal asterisk (not emphasis) |
| `\\` | `\` | Literal backslash |

## Parsing Rules

### Inheritance

Properties flow downward through the hierarchy. Each level can override its parent:

1. **Front matter** sets document-level defaults (base_wpm, speed_offsets).
2. **Segment** overrides document defaults for WPM, emotion, speaker, archetype.
3. **Block** overrides segment defaults for WPM, emotion, speaker, archetype.
4. **Inline tags** override block defaults for the tagged span only (articulation, energy, melody, volume, speed, delivery mode, emotion).

If a child omits a value, it inherits from its nearest ancestor. Archetype follows the same inheritance: a block inside `Archetype:Motivator` segment inherits `Motivator` unless it declares its own `Archetype:` override.

**Best practice:** Only specify WPM or emotion when they **differ** from the inherited value. If `base_wpm` is 140 and a segment runs at 140 WPM, omit the WPM parameter — `## [Intro|Warm]` not `## [Intro|140WPM|Warm]`. Similarly, if a block's emotion matches its parent segment, omit it — `### [Details]` not `### [Details|140WPM|Warm]`. Redundant declarations add noise and make overrides harder to spot.

### WPM Resolution

For any word, the effective WPM is determined by (highest priority first):

1. Inline speed tag (`[150WPM]...[/150WPM]`, `[xslow]`, `[slow]`, `[normal]`, `[fast]`, `[xfast]`)
2. Block header WPM
3. Segment header WPM
4. Archetype recommended WPM (if `Archetype:` is set and no explicit WPM override exists)
5. `base_wpm` from front matter
6. Default: 140 WPM

For example, `## [Rally|Archetype:Motivator]` without an explicit WPM uses 155 WPM (Motivator's recommended). But `## [Rally|150WPM|Archetype:Motivator]` uses 150 WPM — the explicit override wins.

### Pause Handling

- `/` inserts a 300ms pause at the word boundary.
- `//` inserts a 600ms pause at the word boundary.
- `[pause:Ns]` inserts N×1000 ms pause.
- `[pause:Nms]` inserts N ms pause.
- Pauses at word boundaries are additive if multiple markers appear together.

### Emotion Transitions

When emotion changes between segments or blocks, renderers should apply a smooth visual transition (recommended: 3-second fade between color schemes).

### Tag Precedence

**Emotion:** When inline emotion tags are nested, the **innermost tag wins** for the enclosed span. Block-level emotion serves as the default styling; inline emotion tags override it for their span only.

**Volume vs. emotion:** Volume and emotion are **independent dimensions**. `[soft]` inside an `Urgent` block means: deliver with urgent tone but at lower volume. They do not conflict — volume controls loudness, emotion controls tone.

**Delivery modes** (`[sarcasm]`, `[aside]`, `[rhetorical]`, `[building]`) override the current emotion for their span. They are specialized delivery instructions that take priority over the block emotion.

**Volume + delivery modes:** Volume tags apply independently to delivery modes. `[building][loud]text[/loud][/building]` starts at loud volume and gradually increases energy while maintaining the loud level. Volume controls loudness; delivery modes control the shape of delivery.

**Articulation:** When `[legato]` and `[staccato]` are nested, the **innermost tag wins**. They are mutually exclusive — a word is either legato or staccato, never both.

**Energy and melody:** When `[energy:N]` or `[melody:N]` tags are nested, the **innermost value wins** for the enclosed span. These tags set an absolute level (1–10), not a relative offset.

**Archetype vs. inline tags:** Inline tags always take precedence over archetype expectations. An `[energy:2]` inside `Archetype:Motivator` is valid — the word gets energy 2, but the validator emits a warning because Motivator expects 7–10.

### Phrase Boundaries

A **phrase** is a unit of text delimited by:
- Sentence-ending punctuation: `.` `?` `!`
- Pause markers: `/`, `//`, `[pause:...]`
- Block or segment boundaries

Phrases are the smallest unit for timing calculation.

**WPM word counting** is performed on **clean text** — after all tags and markup are stripped. The parser first removes all tag syntax (`[tag]`, `[/tag]`, `[tag:param]`), then joins any text fragments split by mid-word tags (e.g., `develop[stress]me[/stress]nt` → `development`), then counts words using whitespace tokenization. Each whitespace-separated token in the clean text counts as one word. Hyphenated words (e.g., `state-of-the-art`) count as one word.

### Tag Nesting

- Tags must be properly closed: `[loud]text[/loud]`.
- Tags must not cross-nest: `[loud][emphasis]text[/loud][/emphasis]` is **invalid**.
- Valid nesting: `[loud][emphasis]text[/emphasis][/loud]`.
- If a tag is never closed, the parser should implicitly close it at the end of the current block.

### Nested Speed Resolution

When speed tags are nested, relative tags (`[slow]`, `[fast]`, etc.) stack multiplicatively — each multiplier compounds on the previous:

- `[slow]text[/slow]` = base × 0.8
- `[xslow][slow]text[/slow][/xslow]` = base × 0.6 × 0.8 = base × 0.48

When an absolute speed tag (`[150WPM]`) contains a relative tag, the absolute value becomes the new base for the inner tag:

- `[150WPM][slow]text[/slow][/150WPM]` = 150 × 0.8 = 120 WPM

### Content Without Segments

If a TPS file has no `##` segment headers, the entire content (after front matter) is treated as a single implicit segment with `Neutral` emotion and default WPM.

### Simple Headers

Plain markdown `## Title` and `### Title` headers (without `[...]` brackets) are also recognized as segments and blocks respectively, with default (neutral) emotion and inherited WPM.

## Rendering Principles

A TPS file is **source markup**, not display output. The teleprompter application **must** process the markup and present clean, styled text to the reader. The reader should never see raw tags like `[emphasis]` or `[slow]` on screen.

### Tag Visibility

All tags (`[emphasis]`, `[slow]`, `[loud]`, `[sarcasm]`, etc.) are **invisible in the rendered output**. The renderer applies their effects visually:

- `[emphasis]word[/emphasis]` → the word appears **bold** or in a distinct color
- `[slow]text[/slow]` → text may appear with a pacing indicator or wider spacing
- `[loud]text[/loud]` → text appears larger or bolder
- `[whisper]text[/whisper]` → text appears smaller or lighter
- `[sarcasm]text[/sarcasm]` → text styled with a distinct visual cue (e.g., italic + indicator)
- `[building]text[/building]` → text may gradually increase in size or intensity
- `[legato]text[/legato]` → words appear closer together, smooth underline
- `[staccato]text[/staccato]` → words appear spaced apart, dotted underline
- `[energy:N]text[/energy]` → text size/weight scales with energy level
- `[melody:N]text[/melody]` → wave or flat indicator based on level
- `[breath]` → a small visual indicator (e.g., a subtle mark or gap)
- Segment/block headers → rendered as section dividers, not raw markdown

The reader sees only the spoken text with visual styling applied. Tags are commands for the renderer, not content for the speaker.

### Content Guidelines

TPS scripts contain **spoken text only**. Authors should avoid including non-spoken content. Parsers treat everything as plain text — these are authoring guidelines, not parsing constraints.

- **URLs or hyperlinks** — not spoken content; rewrite as "visit our website" or similar
- **Code blocks or technical syntax** — rewrite as spoken language
- **Images or embedded media references** — describe verbally instead
- **Raw data tables** — narrate the data instead

### Rendering Context

TPS is designed for **teleprompter use** — text is always rendered on a **dark background** (typically near-black: `#1A1B2E` or similar). All visual choices assume this context.

### Visual Rendering Hints

Renderers should use **text size, letter spacing, weight, and animation** to communicate delivery cues without the reader needing to see tags. Recommended visual mappings:

| Tag | Font size | Letter spacing | Weight | Other |
|-----|-----------|---------------|--------|-------|
| `[loud]` | Larger (120–140%) | Normal | Bold | — |
| `[soft]` | Smaller (80–90%) | Normal | Light | Lower opacity |
| `[whisper]` | Smaller (70–80%) | Wider (+1–2px) | Light | Italic or distinct style |
| `[emphasis]` | Normal | Normal | Bold | — |
| `[stress]` (wrap) | Larger on stressed part | Normal | Bold | Underline or distinct color |
| `[stress:...]` (guide) | Normal | Normal | Normal | Tooltip/overlay with syllable guide |
| `[slow]` | Normal | Wider (+1–2px) | Normal | Stretches word visually |
| `[fast]` | Normal | Tighter (-0.5–1px) | Normal | Compresses word visually |
| `[building]` | Gradually increasing | Normal | Gradually bolder | Animated size ramp |
| `[sarcasm]` | Normal | Normal | Normal | Italic + visual indicator |
| `[aside]` | Smaller (85–90%) | Normal | Light | Dimmed or offset |
| `[highlight]` | Normal | Normal | Normal | Background overlay |
| `[legato]` | Normal | Tighter (-0.5px) | Normal | Smooth underline or wave |
| `[staccato]` | Normal | Wider (+1–2px) | Bold | Dotted underline or dashes between words |
| `[energy:1–4]` | Smaller (90%) | Normal | Light | Lower opacity |
| `[energy:5–6]` | Normal | Normal | Normal | Default styling (no special treatment) |
| `[energy:7–10]` | Larger (110–130%) | Normal | Bold | Glow or pulsing effect |
| `[melody:1–3]` | Normal | Normal | Normal | Flat visual indicator (dash) |
| `[melody:4–6]` | Normal | Normal | Normal | Default styling (no special treatment) |
| `[melody:7–10]` | Normal | Normal | Normal | Wave or oscillating indicator |

These are **recommendations**, not requirements. Renderers may adapt the visual treatment to their platform. The key principle: the reader should **feel** the delivery instruction from the visual presentation alone.

### Dark Background Rules

1. **Text base color** is white/light (`#F8F9FA` or similar).
2. **Minimum contrast** — all styled text must produce at least **WCAG AA 4.5:1** contrast ratio against the dark background.
3. **Emotion color schemes** (background, text, accent) are pre-defined per emotion and tuned for the dark rendering context.
4. **`highlight`** uses a semi-transparent yellow background overlay, not a text color change.

## WPM Guidelines

### Actor Profile (Spoken Reading)

The Actor profile targets natural spoken delivery — reading aloud from a teleprompter while maintaining eye contact with the camera.

| Use Case | WPM Range | Recommendation |
|----------|-----------|----------------|
| Slow, formal (educational, legal) | 100–120 | Clear diction, long pauses between phrases |
| **Standard teleprompter** | **125–150** | **Comfortable for most speakers** |
| Conversational / energetic | 145–165 | Natural speech rhythm, experienced readers |
| Fast presentation (experienced only) | 160–170 | Upper limit for teleprompter delivery |

**Default `base_wpm`: 140** — sits in the middle of the standard range. Most people read comfortably at 130–150 WPM from a teleprompter.

**Duration formula:** `phrase_duration_ms = (word_count / effective_wpm) × 60000`. Total duration is the sum of all phrase durations plus all pause durations.

**Advice for script authors:**
- Start at 130 WPM for new speakers, increase gradually.
- Use `[xslow]...[/xslow]` for critical warnings or very important statements.
- Use `[slow]...[/slow]` for key points and emotional moments.
- Use `[fast]...[/fast]` for asides and throwaway lines.
- Use `[xfast]...[/xfast]` for rapid transitions or low-importance filler.
- Add `/` pauses between clauses; `//` between sentences.

## Profiles and Validation

### Validation Requirements

| Rule | Value |
|------|-------|
| WPM error range | < 80 or > 220 |
| WPM warning range | < 90 or > 200 |
| Recommended WPM | 125–150 |
| Segment/Block WPM | Single integer |
| Inline speed | Integer, respects allowed range |

Additional validation:
- Emotions must be from the predefined closed set (see Emotions table). Unknown emotion keywords are a parse error.
- Archetypes must be from the predefined closed set (`Friend`, `Motivator`, `Educator`, `Coach`, `Storyteller`, `Entertainer`). Unknown archetype keywords are a parse error.
- Energy values must be integers from 1 to 10. Values outside this range are a parse error.
- Melody values must be integers from 1 to 10. Values outside this range are a parse error.
- Articulation styles must be `legato` or `staccato`. Unknown articulation keywords are treated as unknown tags.
- Archetype validation produces **warnings** (not errors) when inline tags conflict with the archetype's expected profile.
- Timing calculations should not exceed target duration by > 20%.
- All markup tags must be properly closed; no cross-nesting.
- Edit points should be at phrase or block boundaries.
- Pronunciation guides should use valid IPA or phonetic notation.

### Casing, Whitespace, Escaping

- Tags are case-insensitive; canonical form is lower-case (e.g., `[emphasis]`).
- `WPM` suffix is uppercase by convention (e.g., `140WPM`). `140wpm` is also valid — parsers should normalize.
- Parameters inside headers are trimmed; `140 WPM` normalizes to `140WPM`.
- Escape reserved characters in plain text with backslash: `\[`, `\]`, `\|`, `\/`, `\*`, `\\`.
- Escape sequences apply **only in plain text**, not inside tag parameters or header parameters.

### Error Handling

Parsers should handle invalid input gracefully:

- **Unknown tag:** Treat as plain text (display the brackets and content literally).
- **Unknown emotion keyword:** Parse error. Reject the keyword and use the inherited emotion.
- **Unclosed tag:** Implicitly close at the end of the current block (see Tag Nesting).
- **Closing tag without opening:** Ignore the closing tag.
- **Invalid WPM** (< 80 or > 220): Parse error. Use the inherited WPM value.
- **Invalid energy value** (outside 1–10 or non-integer): Parse error. Ignore the tag.
- **Invalid melody value** (outside 1–10 or non-integer): Parse error. Ignore the tag.
- **Unknown archetype keyword**: Parse error. Ignore the `Archetype:` parameter, use no archetype.
- **Archetype profile mismatch**: Warning. Inline tags that conflict with the archetype's expected profile produce warnings, not errors.
- **Malformed pause** (e.g., `[pause:abc]`): Treat as plain text.
- **Cross-nested tags:** Parse error. The parser should close tags in order and report a warning.
- **Duplicate parameters in headers** (e.g., two WPM values): Use the last one.

## Complete Example

```markdown
---
title: Product Launch
profile: Actor
base_wpm: 140
speed_offsets:
  xslow: -40
  slow: -20
  fast: 25
  xfast: 50
author: Jane Doe
---

# Product Launch

## [Intro|Warm|Archetype:Friend]

### [Opening Block]
[legato][energy:5][melody:7]Good morning everyone, / and [emphasis]welcome[/emphasis] to what I believe /
will be a [emphasis]transformative moment[/emphasis] for our company.[/melody][/energy][/legato] //

[pause:2s]

### [Purpose Block|Archetype:Motivator]
[legato][energy:8][melody:8][emphasis]Today[/emphasis], / we're not just launching a product – / [breath]
we're introducing a [highlight]solution[/highlight] that will [emphasis]revolutionize[/emphasis] /
how our customers interact with tech[stress]no[/stress]logy.[/melody][/energy][/legato] //

## [Problem|Concerned|Archetype:Educator]

### [Statistics Block|Neutral]
[energy:3][melody:3]But first, / let's address the [xslow]elephant in the room[/xslow]. /
Our industry has been [emphasis]struggling[/emphasis] with a fundamental problem.[/melody][/energy] //

[edit_point:high]

[energy:4]According to recent studies, /
[slow][emphasis]73% of users a[stress]ban[/stress]don[/emphasis] applications within the first three interactions[/slow] /
due to [highlight]complexity and poor user experience[/highlight].[/energy] //

### [Impact Block]
This affects [emphasis]millions[/emphasis] of people worldwide, /
costing businesses [loud][emphasis]billions[/emphasis] in revenue[/loud] annually. //

## [Solution|Focused]

### [Introduction Block]
That's where our [emphasis]new platform[/emphasis] comes in. /
[building]We've developed a local-first teleprompter workflow that /
[highlight]simplifies complex processes[/highlight] and [emphasis]enhances user experience[/emphasis].[/building] //

### [Benefits Block|150WPM|Excited]
With our solution, / you can expect a [emphasis]50% reduction[/emphasis] in user abandonment /
and a [emphasis]30% increase[/emphasis] in engagement. //

[pause:1s]

[aside]Full details are available in the handout.[/aside] /
[highlight]Thank you[/highlight] for your time. //

## [Action Items|Focused|Archetype:Coach]

### [Next Steps]
[staccato][energy:8][melody:2][loud]**Three** things. **Today.** //
**One** — download the **beta**. //
**Two** — run the **benchmark**. //
**Three** — share your **feedback**.[/loud][/melody][/energy][/staccato]

## [Closing|Warm|Archetype:Friend]

### [Thank You]
[legato][energy:5][melody:7]Thank you all / for being here today. /
I genuinely appreciate your time and attention.[/melody][/energy][/legato] //

[edit_point:medium]
```

## Examples

The [`examples/`](examples/) directory contains sample TPS files demonstrating the format:

| File | Description |
|------|-------------|
| [`basic.tps`](examples/basic.tps) | Minimal valid TPS file — front matter, title, segments, blocks, pauses, emphasis, simple headers, escape sequences. |
| [`advanced.tps`](examples/advanced.tps) | All format features — speed, volume, delivery modes, syllable stress, breath marks, emotions, pronunciation, edit points, tag nesting. |
| [`multi-segment.tps`](examples/multi-segment.tps) | Multi-segment script with varying speed, emotion, and delivery cues across segments. |

## File Extension

- Primary: `.tps` (TelePrompterScript)
- Alternative: `.tps.md` (for markdown-aware editors)
- Alternative: `.md.tps`

## AI Skills

If you use an AI coding assistant (Claude Code, ChatGPT, Cursor, Windsurf, or any LLM-powered tool), you can install TPS skills to let the AI convert plain text into richly formatted `.tps` scripts automatically.

The [`Skills/`](Skills/) directory contains ready-to-use AI skills:

| Skill | Description |
|-------|-------------|
| [`tps-convert`](Skills/tps-convert.md) | Converts plain text (speeches, articles, narrations, dialogues) into fully formatted TPS files with dramatic pacing, emotions, pauses, emphasis, volume, delivery modes, pronunciation guides, stress markers, breath marks, edit points, and speaker tags — all applied intelligently based on content analysis. |

### Installation

**Claude Code** — copy the skill file into your project's `.claude/skills/` directory:

```bash
mkdir -p .claude/skills
cp Skills/tps-convert.md .claude/skills/
```

Then use `/tps-convert` in Claude Code followed by your text.

**Codex** — copy the skill into a repo-local `.codex/skills/` folder as a standard skill:

```bash
mkdir -p .codex/skills/tps-convert
cp Skills/tps-convert.md .codex/skills/tps-convert/SKILL.md
```

**GitHub Copilot** — copy the skill into a repo prompt file or use it as custom instructions:

```bash
mkdir -p .github/prompts
cp Skills/tps-convert.md .github/prompts/tps-convert.prompt.md
```

**Other AI assistants** — paste the contents of the skill file as a system prompt or custom instruction. The skill contains the complete TPS format specification, so the AI will know how to format scripts correctly.

### What the Skill Does

1. **Analyzes** your plain text for tone, emotional arcs, key moments, and structure
2. **Structures** the text into TPS segments (`##`) and blocks (`###`) based on natural topic/mood shifts
3. **Applies** the full TPS tag vocabulary: emotions, speed, volume, delivery modes, emphasis, highlights, pauses, breath marks, edit points, pronunciation, stress, and speaker assignments
4. **Generates** complete front matter with metadata, duration estimation, and speed offsets
5. **Outputs** a valid `.tps` file ready for any TPS-compatible teleprompter

## SDK

TPS includes the `ManagedCode.Tps` SDK workspace.

- **SDK catalog page:** [tps.managed-code.com/sdk](https://tps.managed-code.com/sdk/)
- **Workspace overview:** [SDK/README.md](https://github.com/managedcode/TPS/blob/main/SDK/README.md)
- **TypeScript:** [SDK/ts](https://github.com/managedcode/TPS/tree/main/SDK/ts)
- **JavaScript:** [SDK/js](https://github.com/managedcode/TPS/tree/main/SDK/js)
- **.NET / C#:** [SDK/dotnet](https://github.com/managedcode/TPS/tree/main/SDK/dotnet)
- **Flutter:** [SDK/flutter](https://github.com/managedcode/TPS/tree/main/SDK/flutter)
- **Swift:** [SDK/swift](https://github.com/managedcode/TPS/tree/main/SDK/swift)
- **Java:** [SDK/java](https://github.com/managedcode/TPS/tree/main/SDK/java)
