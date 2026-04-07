using PrompterOne.Core.Models.CompiledScript;
using PrompterOne.Core.Services;

namespace PrompterOne.Core.Tests;

public sealed class TpsPhraseBoundaryTests
{
    [Test]
    [Arguments("[breath]", "IsBreath")]
    [Arguments("[edit_point]", "IsEditPoint")]
    [Arguments("[edit_point:high]", "IsEditPoint")]
    public async Task CompileAsync_SplitsPhrasesWhenControlWordsAppearBetweenSpokenWords(
        string controlWord,
        string controlFlag)
    {
        var compiled = await CompileAsync(
            $$"""
            ---
            title: "Phrase boundary"
            base_wpm: 140
            ---

            ## [Signal|focused]

            ### [Reader Block]

            hello {{controlWord}} hello
            """);

        var segment = Assert.Single(compiled.Segments);
        var block = Assert.Single(segment.Blocks.Where(candidate => candidate.Words.Count > 0));

        Assert.Equal(2, block.Phrases.Count);
        Assert.Equal(["hello"], block.Phrases[0].Words.Select(word => word.CleanText).ToArray());
        Assert.Equal(["hello"], block.Phrases[1].Words.Select(word => word.CleanText).ToArray());
        Assert.Equal(["hello", string.Empty, "hello"], block.Words.Select(word => word.CleanText).ToArray());
        Assert.Contains(
            block.Words,
            word => string.Equals(
                controlFlag,
                nameof(WordMetadata.IsBreath),
                StringComparison.Ordinal)
                ? word.Metadata.IsBreath
                : word.Metadata.IsEditPoint);
    }

    private static async Task<CompiledScript> CompileAsync(string source)
    {
        var documentReader = new TpsDocumentReader();
        var compiler = new ScriptCompiler();
        var document = await documentReader.ReadAsync(source);
        return await compiler.CompileAsync(document);
    }
}
