using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public static partial class TpsRuntime
{
    public static TpsValidationResult Validate(string source)
    {
        var analysis = TpsParser.Parse(source);
        CompileAnalysis(analysis);
        return new TpsValidationResult
        {
            Ok = !TpsSupport.HasErrors(analysis.Diagnostics),
            Diagnostics = analysis.Diagnostics.ToArray()
        };
    }

    public static TpsParseResult Parse(string source)
    {
        var analysis = TpsParser.Parse(source);
        CompileAnalysis(analysis);
        return new TpsParseResult
        {
            Ok = !TpsSupport.HasErrors(analysis.Diagnostics),
            Diagnostics = analysis.Diagnostics.ToArray(),
            Document = analysis.Document
        };
    }

    public static TpsCompilationResult Compile(string source)
    {
        var analysis = TpsParser.Parse(source);
        var script = CompileAnalysis(analysis);
        return new TpsCompilationResult
        {
            Ok = !TpsSupport.HasErrors(analysis.Diagnostics),
            Diagnostics = analysis.Diagnostics.ToArray(),
            Document = analysis.Document,
            Script = CompiledScriptNormalizer.Normalize(script)
        };
    }
}
