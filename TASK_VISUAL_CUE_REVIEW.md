# TPS Visual Cue Screenshot Review

Date: 2026-04-14

Scope: README TPS cue screenshots generated from `tests/TestData/Scripts/test-tps-cue-matrix.tps`, plus the active teleprompter cue card screenshots in `output/readme-screenshot-audit`.

## Own Review

- High: Pronunciation and phonetic screenshots still put the IPA guide in a secondary pill above the word; the guide should be the main reader text and the original spelling should become secondary context.
- High: Building/crescendo and legato marks are too small and read like stray underlines; phrase examples do not prove one continuous mark across the full scoped phrase.
- High: Stress, `[emphasis]`, Markdown bold, and professional/speaker-looking words collapse into bright bold text at README scale.
- Medium: Edit point examples look unstyled and identical even though low, medium, and high priorities should be visibly distinguishable.
- Medium: Several emotion and contour colors reuse the same warm/cool lanes, making cards understandable only by reading labels.
- Medium: Archetype metadata should not carry decorative reader styling; it belongs to semantic context for graph/LLM/assistant use.

## Claude Review

- High: Pronunciation/phonetic guide hierarchy is inverted in cards 40 and 41; IPA is secondary while original spelling remains dominant.
- High: Legato phrase scope breaks after the first word in card 52, unlike the single-word legato card 32.
- High: Stress, emphasis, professional emotion, and speaker metadata are visually identical in cards 37, 42, 23, and 46.
- High: Warm, energetic, and motivational emotion tints are indistinguishable in cards 16, 22, and 26.
- Medium: Melody collides with the same warm/gold family as warm, energetic, motivational, and building.
- Medium: Excited and concerned share the same purple hue family.
- Medium: Rhetorical delivery is effectively invisible; soft volume is too close to normal; archetype appears decorated even though it should not be a reader cue.
- Low: Xslow is much more dramatic than xfast, Markdown bold and italic share the same gold tint, and focused emotion shares green with energy contour.

## Copilot CLI Review

- High: Pronunciation and phonetic guides are not reader-useful in cards 40 and 41 because the guide is a tiny pill and original spelling dominates.
- High: Emotion families collapse into similar warm and cool clusters, including warm/happy/energetic/motivational and sad/calm/focused.
- Medium: Neutral reads like a mood treatment instead of no mood.
- Medium: Stress, emphasis, and Markdown bold are too close at contact-sheet scale.
- Medium: Building/crescendo looks accidental and under-scoped; legato phrase coverage is still ambiguous.
- Medium: Aside, rhetorical, and sarcasm are not visually separated enough.
- Low: Archetype should not be a decorative reader cue.

## Gemini CLI Review

- `gemini-3-pro-preview` could not complete due model capacity exhaustion.
- `gemini-2.5-flash` completed but reported that this CLI session could not visually inspect the PNG images, so it could not provide a real screenshot review.

## Remediation Checklist

- Make pronunciation and phonetic guide text the primary visible reader word, with original spelling as a compact secondary annotation.
- Strengthen building/crescendo and legato CSS so single-word and phrase scopes get one full-width/full-phrase mark.
- Add objective browser assertions for pronunciation hierarchy, stress marker distinctness, edit-point priority styling, and group-level scope marks.
- Separate stress, editorial emphasis, Markdown bold, Markdown italic, and highlight with distinct visual treatments.
- Add visible edit-point priority markers for standard, medium, and high.
- Keep archetype metadata free of decorative word styling and document that it is semantic context, not a reader cue.
- Improve the most-colliding colors and delivery treatments enough that the generated contact sheets are distinguishable at README scale.
