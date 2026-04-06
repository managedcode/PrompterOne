using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Components;

public sealed record AppOnboardingStepDescriptor(
    AppOnboardingStepId StepId,
    UiIconKind IconKind,
    UiTextKey StepLabelKey,
    UiTextKey EyebrowKey,
    UiTextKey TitleKey,
    UiTextKey BodyKey,
    IReadOnlyList<UiTextKey> BulletKeys,
    UiTextKey ActionKey,
    string Route);
