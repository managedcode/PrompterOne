using System.Globalization;
using PrompterOne.Core.Models.CompiledScript;
using PrompterOne.Core.Models.Documents;
using PrompterOne.Core.Models.Tps;

namespace PrompterOne.Core.Services;

public sealed class TpsParser
{
    public async Task<TpsDocument> ParseFileAsync(string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        return await ParseAsync(content).ConfigureAwait(false);
    }

    public Task<TpsDocument> ParseAsync(string tpsContent)
    {
        return Task.FromResult(ParseDocument(tpsContent));
    }

    public ScriptData ParseTps(string tpsContent)
    {
        var document = ParseDocument(tpsContent);
        var compiled = TpsCompilerCore.Compile(document);
        return BuildScriptData(document, compiled, NormalizeLineEndings(tpsContent));
    }

    private static TpsDocument ParseDocument(string? tpsContent)
    {
        var normalizedText = NormalizeLineEndings(tpsContent);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return new TpsDocument();
        }

        var (metadata, body) = ExtractFrontMatter(normalizedText);
        var contentBody = ExtractTitleHeader(metadata, body);
        var segments = ParseSegments(contentBody, metadata);

        return new TpsDocument
        {
            Metadata = metadata,
            Segments = segments
        };
    }

    private static (Dictionary<string, string> Metadata, string Body) ExtractFrontMatter(string text)
    {
        if (!text.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), text);
        }

        var endDelimiterIndex = text.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (endDelimiterIndex < 0)
        {
            return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), text);
        }

        var frontMatterText = text[4..endDelimiterIndex];
        var body = text[(endDelimiterIndex + 5)..];
        return (ParseMetadata(frontMatterText), body);
    }

    private static Dictionary<string, string> ParseMetadata(string frontMatterText)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;

        foreach (var rawLine in frontMatterText.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var indentationLength = rawLine.Length - rawLine.TrimStart().Length;
            var line = rawLine.Trim();
            if (line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = NormalizeMetadataValue(line[(separatorIndex + 1)..]);
            if (indentationLength > 0 && !string.IsNullOrWhiteSpace(currentSection))
            {
                var compositeKey = string.Concat(currentSection, ".", key);
                if (!IsLegacyMetadataKey(compositeKey))
                {
                    metadata[compositeKey] = value;
                }

                continue;
            }

            currentSection = string.IsNullOrWhiteSpace(value) ? key : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!IsLegacyMetadataKey(key))
            {
                metadata[key] = value;
            }
        }

        return metadata;
    }

    private static string ExtractTitleHeader(Dictionary<string, string> metadata, string body)
    {
        var lines = body.Split('\n');
        var lineIndex = 0;
        while (lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex]))
        {
            lineIndex++;
        }

        if (lineIndex >= lines.Length)
        {
            return body;
        }

        var candidate = lines[lineIndex].Trim();
        if (!candidate.StartsWith("# ", StringComparison.Ordinal) || candidate.StartsWith("##", StringComparison.Ordinal))
        {
            return body;
        }

        metadata[TpsSpec.FrontMatterKeys.Title] = candidate[2..].Trim();
        lines[lineIndex] = string.Empty;
        return string.Join('\n', lines).TrimStart('\n');
    }

    private static List<TpsSegment> ParseSegments(string body, IReadOnlyDictionary<string, string> metadata)
    {
        var segments = new List<TpsSegment>();
        var preambleBuilder = new List<string>();
        TpsSegment? currentSegment = null;
        TpsBlock? currentBlock = null;
        var currentSegmentLeadingLines = new List<string>();
        var currentBlockLines = new List<string>();

        foreach (var rawLine in body.Split('\n'))
        {
            if (TryParseHeader(rawLine, HeaderLevel.Segment, out var segmentHeader))
            {
                FlushBlock(currentSegment, currentBlock, currentBlockLines);
                FlushSegment(segments, currentSegment, currentSegmentLeadingLines);

                currentSegment = CreateSegment(segmentHeader, metadata);
                currentBlock = null;
                if (preambleBuilder.Count > 0)
                {
                    currentSegmentLeadingLines.AddRange(preambleBuilder);
                    preambleBuilder.Clear();
                }

                continue;
            }

            if (TryParseHeader(rawLine, HeaderLevel.Block, out var blockHeader))
            {
                if (currentSegment is null)
                {
                    currentSegment = CreateImplicitSegment(metadata);
                    currentSegmentLeadingLines.AddRange(preambleBuilder);
                    preambleBuilder.Clear();
                }

                FlushBlock(currentSegment, currentBlock, currentBlockLines);
                currentBlock = CreateBlock(blockHeader);
                continue;
            }

            if (currentBlock is not null)
            {
                currentBlockLines.Add(rawLine);
            }
            else if (currentSegment is not null)
            {
                currentSegmentLeadingLines.Add(rawLine);
            }
            else
            {
                preambleBuilder.Add(rawLine);
            }
        }

        if (currentSegment is null)
        {
            var implicitSegment = CreateImplicitSegment(metadata);
            implicitSegment.LeadingContent = NormalizeBody(string.Join('\n', preambleBuilder));
            implicitSegment.Content = implicitSegment.LeadingContent ?? string.Empty;
            segments.Add(implicitSegment);
            return segments;
        }

        FlushBlock(currentSegment, currentBlock, currentBlockLines);
        FlushSegment(segments, currentSegment, currentSegmentLeadingLines);
        return segments;
    }

    private static void FlushBlock(TpsSegment? currentSegment, TpsBlock? currentBlock, List<string> currentBlockLines)
    {
        if (currentSegment is null || currentBlock is null)
        {
            currentBlockLines.Clear();
            return;
        }

        currentBlock.Content = NormalizeBody(string.Join('\n', currentBlockLines));
        currentSegment.Blocks.Add(currentBlock);
        currentBlockLines.Clear();
    }

    private static void FlushSegment(List<TpsSegment> segments, TpsSegment? currentSegment, List<string> currentSegmentLeadingLines)
    {
        if (currentSegment is null)
        {
            currentSegmentLeadingLines.Clear();
            return;
        }

        currentSegment.LeadingContent = NormalizeBody(string.Join('\n', currentSegmentLeadingLines));
        currentSegment.Content = currentSegment.Blocks.Count == 0
            ? currentSegment.LeadingContent ?? string.Empty
            : string.Empty;
        segments.Add(currentSegment);
        currentSegmentLeadingLines.Clear();
    }

    private static TpsSegment CreateSegment(ParsedHeader header, IReadOnlyDictionary<string, string> metadata)
    {
        var emotion = ResolveEmotion(header.Emotion, TpsSpec.DefaultEmotion);
        var palette = ResolvePalette(emotion);
        return new TpsSegment
        {
            Name = header.Name,
            TargetWPM = header.TargetWpm ?? ResolveBaseWpm(metadata),
            Emotion = emotion,
            Speaker = header.Speaker,
            Timing = header.Timing,
            BackgroundColor = palette.Background,
            TextColor = palette.Text,
            AccentColor = palette.Accent
        };
    }

    private static TpsBlock CreateBlock(ParsedHeader header)
    {
        return new TpsBlock
        {
            Name = header.Name,
            TargetWPM = header.TargetWpm,
            Emotion = NormalizeValue(header.Emotion)?.ToLowerInvariant(),
            Speaker = header.Speaker
        };
    }

    private static TpsSegment CreateImplicitSegment(IReadOnlyDictionary<string, string> metadata)
    {
        var baseWpm = ResolveBaseWpm(metadata);
        var palette = ResolvePalette(TpsSpec.DefaultEmotion);
        return new TpsSegment
        {
            Name = metadata.TryGetValue(TpsSpec.FrontMatterKeys.Title, out var title) && !string.IsNullOrWhiteSpace(title)
                ? title
                : TpsSpec.DefaultImplicitSegmentName,
            TargetWPM = baseWpm,
            Emotion = TpsSpec.DefaultEmotion,
            BackgroundColor = palette.Background,
            TextColor = palette.Text,
            AccentColor = palette.Accent
        };
    }

    private static bool TryParseHeader(string line, HeaderLevel level, out ParsedHeader header)
    {
        header = ParsedHeader.Empty;
        var trimmedLine = line.Trim();
        var prefix = level == HeaderLevel.Segment ? "## " : "### ";
        if (!trimmedLine.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var headerContent = trimmedLine[prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(headerContent))
        {
            return false;
        }

        if (!headerContent.StartsWith("[", StringComparison.Ordinal) || !headerContent.EndsWith("]", StringComparison.Ordinal))
        {
            header = new ParsedHeader(headerContent, null, null, null, null);
            return true;
        }

        var protectedContent = TpsEscaping.Protect(headerContent[1..^1]);
        var parts = TpsEscaping.SplitHeaderParts(protectedContent);
        if (parts.Count == 0 || string.IsNullOrWhiteSpace(parts[0]))
        {
            return false;
        }

        int? targetWpm = null;
        string? emotion = null;
        string? timing = null;
        string? speaker = null;

        foreach (var rawPart in parts.Skip(1))
        {
            var part = NormalizeValue(rawPart);
            if (part is null)
            {
                continue;
            }

            if (part.StartsWith(TpsSpec.SpeakerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                speaker = NormalizeValue(part[TpsSpec.SpeakerPrefix.Length..]);
                continue;
            }

            if (TryParseHeaderWpm(part, out var parsedWpm))
            {
                targetWpm = parsedWpm;
                continue;
            }

            if (IsTiming(part))
            {
                timing = part;
                continue;
            }

            if (TpsSpec.Emotions.Contains(part))
            {
                emotion = part.ToLowerInvariant();
            }
        }

        header = new ParsedHeader(parts[0], targetWpm, emotion, timing, speaker);
        return true;
    }

    private static bool TryParseHeaderWpm(string value, out int? wpm)
    {
        wpm = null;
        var normalized = value.Trim();
        if (normalized.EndsWith(TpsSpec.WpmSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var numberPart = normalized[..^TpsSpec.WpmSuffix.Length];
            if (int.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWithSuffix))
            {
                wpm = parsedWithSuffix;
            }

            return true;
        }

        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            wpm = parsed;
            return true;
        }

        return false;
    }

    private static bool IsTiming(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var rangeSeparatorIndex = value.IndexOf('-', StringComparison.Ordinal);
        if (rangeSeparatorIndex < 0)
        {
            return IsTimeToken(value);
        }

        return IsTimeToken(value[..rangeSeparatorIndex]) && IsTimeToken(value[(rangeSeparatorIndex + 1)..]);
    }

    private static bool IsTimeToken(string value)
    {
        return TimeSpan.TryParseExact(value.Trim(), ["m\\:ss", "mm\\:ss"], CultureInfo.InvariantCulture, out _);
    }

    private static ScriptData BuildScriptData(TpsDocument document, CompiledScript compiled, string sourceText)
    {
        var baseWpm = ResolveBaseWpm(document.Metadata);
        var segments = new List<ScriptSegment>(document.Segments.Count);
        var nextWordIndex = 0;

        for (var segmentIndex = 0; segmentIndex < document.Segments.Count; segmentIndex++)
        {
            var segment = document.Segments[segmentIndex];
            var compiledSegment = compiled.Segments.ElementAtOrDefault(segmentIndex);
            var blocks = new List<ScriptBlock>(segment.Blocks.Count);
            var segmentStartIndex = nextWordIndex;

            foreach (var (block, blockIndex) in segment.Blocks.Select((value, index) => (value, index)))
            {
                var compiledBlock = compiledSegment?.Blocks.ElementAtOrDefault(blockIndex);
                blocks.Add(BuildScriptBlock(block, compiledBlock, segment.TargetWPM ?? baseWpm, ref nextWordIndex));
            }

            var (startTime, endTime) = SplitTiming(segment.Timing);
            segments.Add(new ScriptSegment
            {
                Name = segment.Name,
                Emotion = ResolveEmotion(segment.Emotion, TpsSpec.DefaultEmotion),
                Speaker = segment.Speaker,
                Timing = segment.Timing,
                BackgroundColor = segment.BackgroundColor,
                TextColor = segment.TextColor,
                AccentColor = segment.AccentColor,
                WpmOverride = segment.TargetWPM,
                StartTime = startTime,
                EndTime = endTime,
                StartIndex = segmentStartIndex,
                EndIndex = Math.Max(segmentStartIndex, nextWordIndex - 1),
                Content = !string.IsNullOrWhiteSpace(segment.LeadingContent) ? segment.LeadingContent! : segment.Content,
                Blocks = blocks.Count == 0 ? null : blocks.ToArray()
            });
        }

        return new ScriptData
        {
            Title = document.Metadata.TryGetValue(TpsSpec.FrontMatterKeys.Title, out var title) ? title : null,
            Content = sourceText,
            TargetWpm = baseWpm,
            Segments = segments.Count == 0 ? null : segments.ToArray()
        };
    }

    private static ScriptBlock BuildScriptBlock(TpsBlock block, CompiledBlock? compiledBlock, int fallbackWpm, ref int nextWordIndex)
    {
        var phrases = BuildScriptPhrases(compiledBlock?.Words ?? [], fallbackWpm, ref nextWordIndex);
        return new ScriptBlock
        {
            Name = block.Name,
            Emotion = NormalizeValue(block.Emotion)?.ToLowerInvariant(),
            Speaker = block.Speaker,
            WpmOverride = block.TargetWPM,
            StartIndex = phrases.Length == 0 ? nextWordIndex : phrases[0].StartIndex,
            EndIndex = phrases.Length == 0 ? nextWordIndex : phrases[^1].EndIndex,
            Content = block.Content,
            Phrases = phrases.Length == 0 ? null : phrases
        };
    }

    private static ScriptPhrase[] BuildScriptPhrases(IEnumerable<CompiledWord> compiledWords, int fallbackWpm, ref int nextWordIndex)
    {
        var phrases = new List<ScriptPhrase>();
        var currentWords = new List<ScriptWord>();
        var currentPhraseStart = nextWordIndex;

        foreach (var compiledWord in compiledWords)
        {
            currentWords.Add(BuildScriptWord(compiledWord, fallbackWpm));
            nextWordIndex++;

            if (compiledWord.Metadata.IsPause || HasSentenceEndingPunctuation(compiledWord.CleanText))
            {
                phrases.Add(CreatePhrase(currentWords, currentPhraseStart, nextWordIndex - 1));
                currentWords.Clear();
                currentPhraseStart = nextWordIndex;
            }
        }

        if (currentWords.Count > 0)
        {
            phrases.Add(CreatePhrase(currentWords, currentPhraseStart, nextWordIndex - 1));
        }

        return phrases.ToArray();
    }

    private static ScriptPhrase CreatePhrase(List<ScriptWord> words, int startIndex, int endIndex)
    {
        return new ScriptPhrase
        {
            Text = string.Join(' ', words.Where(word => !string.IsNullOrWhiteSpace(word.Text)).Select(word => word.Text)),
            StartIndex = startIndex,
            EndIndex = Math.Max(startIndex, endIndex),
            Words = words.ToArray()
        };
    }

    private static ScriptWord BuildScriptWord(CompiledWord compiledWord, int fallbackWpm)
    {
        var effectiveWpm = compiledWord.Metadata.SpeedOverride
            ?? (compiledWord.Metadata.SpeedMultiplier is float multiplier
                ? Math.Max(1, (int)Math.Round(fallbackWpm * multiplier, MidpointRounding.AwayFromZero))
                : (int?)null);

        return new ScriptWord
        {
            Text = compiledWord.CleanText,
            OrpIndex = compiledWord.ORPPosition,
            WpmOverride = effectiveWpm,
            SpeedMultiplier = compiledWord.Metadata.SpeedMultiplier,
            EmphasisLevel = compiledWord.Metadata.EmphasisLevel,
            IsHighlight = compiledWord.Metadata.IsHighlight,
            IsBreath = compiledWord.Metadata.IsBreath,
            Emotion = NormalizeValue(compiledWord.Metadata.InlineEmotionHint ?? compiledWord.Metadata.EmotionHint)?.ToLowerInvariant(),
            VolumeLevel = NormalizeValue(compiledWord.Metadata.VolumeLevel)?.ToLowerInvariant(),
            DeliveryMode = NormalizeValue(compiledWord.Metadata.DeliveryMode)?.ToLowerInvariant(),
            PauseAfter = compiledWord.Metadata.PauseDuration,
            Pronunciation = compiledWord.Metadata.PronunciationGuide,
            StressText = compiledWord.Metadata.StressText,
            StressGuide = compiledWord.Metadata.StressGuide,
            Speaker = compiledWord.Metadata.Speaker,
            IsEditPoint = compiledWord.Metadata.IsEditPoint,
            EditPointPriority = NormalizeValue(compiledWord.Metadata.EditPointPriority)?.ToLowerInvariant()
        };
    }

    private static (string? Start, string? End) SplitTiming(string? timing)
    {
        var trimmed = NormalizeValue(timing);
        if (trimmed is null)
        {
            return (null, null);
        }

        var separatorIndex = trimmed.IndexOf('-', StringComparison.Ordinal);
        if (separatorIndex < 0)
        {
            return (trimmed, trimmed);
        }

        return (trimmed[..separatorIndex], trimmed[(separatorIndex + 1)..]);
    }

    private static string NormalizeMetadataValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 &&
            ((trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal)) ||
             (trimmed.StartsWith("'", StringComparison.Ordinal) && trimmed.EndsWith("'", StringComparison.Ordinal))))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    private static bool IsLegacyMetadataKey(string key)
    {
        return key.Equals(TpsSpec.LegacyKeys.DisplayDuration, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.XslowOffset, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.SlowOffset, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.FastOffset, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.XfastOffset, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.PresetsXslow, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.PresetsSlow, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.PresetsFast, StringComparison.OrdinalIgnoreCase) ||
               key.Equals(TpsSpec.LegacyKeys.PresetsXfast, StringComparison.OrdinalIgnoreCase);
    }

    private static int ResolveBaseWpm(IReadOnlyDictionary<string, string> metadata)
    {
        return metadata.TryGetValue(TpsSpec.FrontMatterKeys.BaseWpm, out var value) &&
               int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? Math.Max(1, parsed)
            : TpsSpec.DefaultBaseWpm;
    }

    private static string ResolveEmotion(string? emotion, string fallback)
    {
        var normalized = NormalizeValue(emotion)?.ToLowerInvariant();
        return normalized is not null && TpsSpec.Emotions.Contains(normalized)
            ? normalized
            : fallback;
    }

    private static EmotionPalette ResolvePalette(string emotion)
    {
        return TpsSpec.EmotionPalettes.TryGetValue(emotion, out var palette)
            ? palette
            : TpsSpec.EmotionPalettes[TpsSpec.DefaultEmotion];
    }

    private static bool HasSentenceEndingPunctuation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var lastCharacter = value.TrimEnd()[^1];
        return lastCharacter is '.' or '!' or '?';
    }

    private static string NormalizeLineEndings(string? value) =>
        value?.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n') ?? string.Empty;

    private static string NormalizeBody(string? value) =>
        NormalizeLineEndings(value).Trim('\n');

    private static string? NormalizeValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private enum HeaderLevel
    {
        Segment,
        Block
    }

    private sealed record ParsedHeader(string Name, int? TargetWpm, string? Emotion, string? Timing, string? Speaker)
    {
        public static ParsedHeader Empty { get; } = new(string.Empty, null, null, null, null);
    }
}
