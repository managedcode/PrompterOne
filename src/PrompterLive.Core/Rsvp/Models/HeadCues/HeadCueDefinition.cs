namespace PrompterLive.Core.Models.HeadCues;

public sealed partial record HeadCueDefinition(
    string Id,
    string Title,
    string Instruction,
    double Pitch,
    double Yaw,
    double Roll,
    string SvgAssetUri);
