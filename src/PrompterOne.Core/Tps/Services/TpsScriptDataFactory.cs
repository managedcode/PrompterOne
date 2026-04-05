using ManagedCode.Tps;
using PrompterOne.Core.Models.Documents;

namespace PrompterOne.Core.Services;

public sealed class TpsScriptDataFactory
{
    public ScriptData Build(string tpsContent)
    {
        var normalizedText = TpsSourceNormalizer.NormalizeLineEndings(tpsContent);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return new ScriptData
            {
                Content = normalizedText,
                TargetWpm = TpsSpec.DefaultBaseWpm
            };
        }

        var result = TpsRuntime.Compile(normalizedText);
        var document = TpsSdkMapper.ToLocalDocument(result.Document);
        var compiled = TpsSdkMapper.ToLocalCompiledScript(result.Script);
        return TpsScriptDataBuilder.Build(document, compiled, normalizedText);
    }
}
