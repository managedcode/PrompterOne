using System.Text;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed partial class TpsContentCompiler
{
    public static ContentCompilationResult Compile(
        string rawText,
        int startOffset,
        InheritedFormattingState inherited,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        var protectedText = TpsEscaping.Protect(rawText);
        var words = new List<WordSeed>();
        var phrases = new List<PhraseSeed>();
        var currentPhrase = new List<WordSeed>();
        var scopes = new List<InlineScope>();
        var literalScopes = new List<string>();
        var builder = new StringBuilder();
        TokenAccumulator? token = null;

        for (var index = 0; index < protectedText.Length; index++)
        {
            var currentCharacter = protectedText[index];
            if (TryHandleMarkdownMarker(protectedText, ref index, scopes))
            {
                FinalizeToken(words, phrases, currentPhrase, builder, ref token, inherited);
                continue;
            }

            if (currentCharacter == '[')
            {
                var tag = ReadTagToken(protectedText, index);
                if (tag is null)
                {
                    diagnostics.Add(TpsSupport.CreateDiagnostic(
                        TpsSpec.DiagnosticCodes.UnterminatedTag,
                        "Tag is missing a closing ] bracket.",
                        startOffset + index,
                        startOffset + protectedText.Length,
                        lineStarts));
                    AppendLiteral(protectedText[index..], scopes, inherited, builder, ref token);
                    break;
                }

                if (RequiresTokenBoundary(tag.Name))
                {
                    FinalizeToken(words, phrases, currentPhrase, builder, ref token, inherited);
                }

                if (HandleTagToken(tag, literalScopes, scopes, words, phrases, currentPhrase, inherited, startOffset + index, lineStarts, diagnostics))
                {
                    index += tag.Raw.Length - 1;
                    continue;
                }

                AppendLiteral(tag.Raw, scopes, inherited, builder, ref token);
                index += tag.Raw.Length - 1;
                continue;
            }

            if (TryHandleSlashPause(protectedText, index, builder, token))
            {
                FinalizeToken(words, phrases, currentPhrase, builder, ref token, inherited);
                FlushPhrase(phrases, currentPhrase);
                var pauseDuration = index + 1 < protectedText.Length && protectedText[index + 1] == '/'
                    ? TpsSpec.MediumPauseDurationMs
                    : TpsSpec.ShortPauseDurationMs;
                words.Add(CreateControlWord(TpsSpec.WordKinds.Pause, inherited, pauseDuration));
                if (pauseDuration == TpsSpec.MediumPauseDurationMs)
                {
                    index++;
                }

                continue;
            }

            if (char.IsWhiteSpace(currentCharacter))
            {
                FinalizeToken(words, phrases, currentPhrase, builder, ref token, inherited);
                continue;
            }

            AppendCharacter(currentCharacter, scopes, inherited, builder, ref token);
        }

        FinalizeToken(words, phrases, currentPhrase, builder, ref token, inherited);
        FlushPhrase(phrases, currentPhrase);
        foreach (var scope in scopes)
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.UnclosedTag,
                $"Tag '{scope.Name}' was opened but never closed.",
                startOffset + rawText.Length,
                startOffset + rawText.Length,
                lineStarts));
        }

        return new ContentCompilationResult(words, phrases);
    }
}
