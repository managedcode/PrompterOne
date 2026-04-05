using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

internal sealed record SegmentCandidate(
    CompiledSegment Segment,
    List<BlockCandidate> Blocks,
    List<CompiledBlock> CompiledBlocks,
    List<CompiledWord> CompiledWords);

internal sealed record BlockCandidate(
    CompiledBlock Block,
    ContentCompilationResult Content,
    List<CompiledPhrase> Phrases,
    List<CompiledWord> Words,
    ArchetypeDiagnosticTarget DiagnosticTarget);

internal sealed record BlockDefinition(TpsBlock Block, bool IsImplicit, ContentSection? Content, int HeaderStart, int HeaderEnd);

internal sealed record FinalizedBlock(List<CompiledWord> Words, List<CompiledPhrase> Phrases, int ElapsedMs, int NextWordIndex);
