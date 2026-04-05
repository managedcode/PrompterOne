using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed partial class TpsParser
{
    public static DocumentAnalysis Parse(string source)
    {
        var normalized = TpsSupport.NormalizeLineEndings(source);
        var lineStarts = TpsSupport.CreateLineStarts(normalized);
        var diagnostics = new List<TpsDiagnostic>();
        var frontMatter = ExtractFrontMatter(normalized, lineStarts, diagnostics);
        var titleBody = ExtractTitleHeader(frontMatter.Body, frontMatter.BodyStartOffset, frontMatter.Metadata);
        var parsedSegments = ParseSegments(titleBody.Body, titleBody.BodyStartOffset, frontMatter.Metadata, lineStarts, diagnostics);

        return new DocumentAnalysis(
            normalized,
            lineStarts,
            diagnostics,
            FreezeDocument(frontMatter.Metadata, parsedSegments),
            parsedSegments);
    }
}
