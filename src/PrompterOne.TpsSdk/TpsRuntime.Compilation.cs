using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public static partial class TpsRuntime
{
    private static CompiledScript CompileAnalysis(DocumentAnalysis analysis)
    {
        var baseWpm = TpsSupport.ResolveBaseWpm(analysis.Document.Metadata);
        var speedOffsets = TpsSupport.ResolveSpeedOffsets(analysis.Document.Metadata);
        var candidates = analysis.ParsedSegments
            .Select(parsedSegment => CompileSegment(parsedSegment, baseWpm, speedOffsets, analysis))
            .ToList();
        var script = FinalizeScript(analysis.Document.Metadata, candidates);
        TpsArchetypeAnalyzer.AppendDiagnostics(candidates.SelectMany(candidate => candidate.Blocks.Select(block => block.DiagnosticTarget)), analysis.LineStarts, analysis.Diagnostics);
        return script;
    }

    private static SegmentCandidate CompileSegment(
        ParsedSegmentInternal parsedSegment,
        int baseWpm,
        IReadOnlyDictionary<string, int> speedOffsets,
        DocumentAnalysis analysis)
    {
        var segmentEmotion = TpsSupport.ResolveEmotion(parsedSegment.Segment.Emotion);
        var inherited = new InheritedFormattingState(parsedSegment.Segment.TargetWpm ?? baseWpm, segmentEmotion, parsedSegment.Segment.Speaker, speedOffsets, parsedSegment.Segment.Archetype);
        var blocks = BuildBlocks(parsedSegment).Select(entry => CompileBlock(entry, inherited, analysis)).ToList();
        var compiledBlocks = new List<CompiledBlock>();
        var compiledWords = new List<CompiledWord>();
        return new SegmentCandidate(
            new CompiledSegment
            {
                Id = parsedSegment.Segment.Id,
                Name = parsedSegment.Segment.Name,
                TargetWpm = inherited.TargetWpm,
                Emotion = segmentEmotion,
                Speaker = parsedSegment.Segment.Speaker,
                Archetype = parsedSegment.Segment.Archetype,
                Timing = parsedSegment.Segment.Timing,
                BackgroundColor = parsedSegment.Segment.BackgroundColor,
                TextColor = parsedSegment.Segment.TextColor,
                AccentColor = parsedSegment.Segment.AccentColor,
                Blocks = compiledBlocks,
                Words = compiledWords
            },
            blocks,
            compiledBlocks,
            compiledWords);
    }

    private static IEnumerable<BlockDefinition> BuildBlocks(ParsedSegmentInternal parsedSegment)
    {
        if (parsedSegment.ParsedBlocks.Count > 0 &&
            parsedSegment.LeadingContent?.Text is { Length: > 0 } leadingContent)
        {
            yield return new BlockDefinition(
                new TpsBlock
                {
                    Id = $"{parsedSegment.Segment.Id}-implicit-lead",
                    Name = $"{parsedSegment.Segment.Name} Lead",
                    Content = leadingContent,
                    TargetWpm = parsedSegment.Segment.TargetWpm,
                    Emotion = parsedSegment.Segment.Emotion,
                    Speaker = parsedSegment.Segment.Speaker,
                    Archetype = parsedSegment.Segment.Archetype
                },
                true,
                parsedSegment.LeadingContent,
                parsedSegment.HeaderStart,
                parsedSegment.HeaderEnd);
        }

        if (parsedSegment.ParsedBlocks.Count == 0)
        {
            yield return new BlockDefinition(
                new TpsBlock
                {
                    Id = $"{parsedSegment.Segment.Id}-implicit-body",
                    Name = parsedSegment.Segment.Name,
                    Content = parsedSegment.DirectContent?.Text ?? string.Empty,
                    TargetWpm = parsedSegment.Segment.TargetWpm,
                    Emotion = parsedSegment.Segment.Emotion,
                    Speaker = parsedSegment.Segment.Speaker,
                    Archetype = parsedSegment.Segment.Archetype
                },
                true,
                parsedSegment.DirectContent,
                parsedSegment.HeaderStart,
                parsedSegment.HeaderEnd);
        }

        foreach (var parsedBlock in parsedSegment.ParsedBlocks)
        {
            yield return new BlockDefinition(parsedBlock.Block, false, parsedBlock.Content, parsedBlock.HeaderStart, parsedBlock.HeaderEnd);
        }
    }

    private static BlockCandidate CompileBlock(BlockDefinition definition, InheritedFormattingState inherited, DocumentAnalysis analysis)
    {
        var blockArchetype = definition.Block.Archetype ?? inherited.Archetype;
        var blockTargetWpm = definition.Block.TargetWpm
            ?? (blockArchetype is not null && TpsSpec.ArchetypeRecommendedWpm.TryGetValue(blockArchetype, out var archetypeWpm) ? archetypeWpm : inherited.TargetWpm);
        var blockInherited = new InheritedFormattingState(
            blockTargetWpm,
            TpsSupport.ResolveEmotion(definition.Block.Emotion, inherited.Emotion),
            definition.Block.Speaker ?? inherited.Speaker,
            inherited.SpeedOffsets,
            blockArchetype);

        var content = TpsContentCompiler.Compile(
            definition.Content?.Text ?? string.Empty,
            definition.Content?.StartOffset ?? 0,
            blockInherited,
            analysis.LineStarts,
            analysis.Diagnostics);
        var phrases = new List<CompiledPhrase>();
        var words = new List<CompiledWord>();

        var compiledBlock = new CompiledBlock
        {
            Id = definition.Block.Id,
            Name = definition.Block.Name,
            TargetWpm = blockInherited.TargetWpm,
            Emotion = blockInherited.Emotion,
            Speaker = blockInherited.Speaker,
            Archetype = blockArchetype,
            IsImplicit = definition.IsImplicit,
            Phrases = phrases,
            Words = words
        };

        return new BlockCandidate(
            compiledBlock,
            content,
            phrases,
            words,
            new ArchetypeDiagnosticTarget(compiledBlock, definition.HeaderStart, definition.HeaderEnd));
    }

    private static CompiledScript FinalizeScript(IReadOnlyDictionary<string, string> metadata, IEnumerable<SegmentCandidate> candidates)
    {
        var scriptMetadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
        var scriptSegments = new List<CompiledSegment>();
        var scriptWords = new List<CompiledWord>();
        var script = new CompiledScript
        {
            Metadata = scriptMetadata,
            Segments = scriptSegments,
            Words = scriptWords
        };
        var elapsedMs = 0;
        var wordIndex = 0;

        foreach (var segmentCandidate in candidates)
        {
            var segmentWords = new List<CompiledWord>();
            foreach (var blockCandidate in segmentCandidate.Blocks)
            {
                var finalizedBlock = FinalizeBlock(blockCandidate.Block, blockCandidate.Content, segmentCandidate.Segment.Id, elapsedMs, wordIndex);
                blockCandidate.Words.AddRange(finalizedBlock.Words);
                blockCandidate.Phrases.AddRange(finalizedBlock.Phrases);
                segmentCandidate.CompiledBlocks.Add(blockCandidate.Block);
                segmentWords.AddRange(finalizedBlock.Words);
                scriptWords.AddRange(finalizedBlock.Words);
                elapsedMs = finalizedBlock.ElapsedMs;
                wordIndex = finalizedBlock.NextWordIndex;
            }

            segmentCandidate.CompiledWords.AddRange(segmentWords);
            ApplyTimeRange(segmentCandidate.Segment, segmentWords);
            scriptSegments.Add(segmentCandidate.Segment);
        }

        return new CompiledScript
        {
            Metadata = script.Metadata,
            Segments = script.Segments,
            Words = script.Words,
            TotalDurationMs = elapsedMs
        };
    }

    private static FinalizedBlock FinalizeBlock(
        CompiledBlock block,
        ContentCompilationResult content,
        string segmentId,
        int elapsedMs,
        int wordIndex)
    {
        var wordMap = new Dictionary<WordSeed, CompiledWord>();
        var words = new List<CompiledWord>();
        foreach (var seed in content.Words)
        {
            var word = new CompiledWord
            {
                Id = $"word-{wordIndex + 1}",
                Index = wordIndex,
                Kind = seed.Kind,
                CleanText = seed.CleanText,
                CharacterCount = seed.CharacterCount,
                OrpPosition = seed.OrpPosition,
                DisplayDurationMs = seed.DisplayDurationMs,
                StartMs = elapsedMs,
                EndMs = elapsedMs + seed.DisplayDurationMs,
                Metadata = seed.Metadata,
                SegmentId = segmentId,
                BlockId = block.Id,
                PhraseId = string.Empty
            };
            wordMap[seed] = word;
            words.Add(word);
            elapsedMs = word.EndMs;
            wordIndex++;
        }

        var phrases = new List<CompiledPhrase>();
        for (var index = 0; index < content.Phrases.Count; index++)
        {
            var sourcePhrase = content.Phrases[index];
            var phraseWords = ResolvePhraseWords(sourcePhrase, wordMap);
            if (phraseWords.Count == 0)
            {
                continue;
            }

            var phrase = new CompiledPhrase
            {
                Id = $"{block.Id}-phrase-{index + 1}",
                Text = sourcePhrase.Text,
                StartWordIndex = phraseWords[0].Index,
                EndWordIndex = phraseWords[^1].Index,
                StartMs = phraseWords[0].StartMs,
                EndMs = phraseWords[^1].EndMs,
                Words = phraseWords
            };

            foreach (var word in phraseWords)
            {
                word.PhraseId = phrase.Id;
            }

            phrases.Add(phrase);
        }

        ApplyTimeRange(block, words);
        return new FinalizedBlock(words, phrases, elapsedMs, wordIndex);
    }

    private static List<CompiledWord> ResolvePhraseWords(
        PhraseSeed phrase,
        IReadOnlyDictionary<WordSeed, CompiledWord> wordMap)
    {
        var phraseWords = new List<CompiledWord>(phrase.Words.Count);
        foreach (var word in phrase.Words)
        {
            if (wordMap.TryGetValue(word, out var compiledWord))
            {
                phraseWords.Add(compiledWord);
            }
        }

        return phraseWords;
    }

    private static void ApplyTimeRange(ICompiledTimeRange target, IReadOnlyList<CompiledWord> words)
    {
        target.StartWordIndex = words.Count == 0 ? 0 : words[0].Index;
        target.EndWordIndex = words.Count == 0 ? target.StartWordIndex : words[^1].Index;
        target.StartMs = words.Count == 0 ? 0 : words[0].StartMs;
        target.EndMs = words.Count == 0 ? target.StartMs : words[^1].EndMs;
    }
}
