namespace PrompterOne.Shared.Settings.Models;

public sealed class AiProviderModelSettings
{
    public string ModelPath { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool HasAnyValue() =>
        !string.IsNullOrWhiteSpace(Name) ||
        !string.IsNullOrWhiteSpace(ModelPath);

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(Name);

    public bool IsConfiguredWithLocalPath() =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(ModelPath);

    public AiProviderModelSettings Normalize()
    {
        Name = Name.Trim();
        ModelPath = ModelPath.Trim();

        if (string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(ModelPath))
        {
            Name = Path.GetFileNameWithoutExtension(ModelPath);
        }

        return this;
    }

    public static AiProviderModelSettings Create(string name, string modelPath = "") =>
        new AiProviderModelSettings
        {
            ModelPath = modelPath,
            Name = name
        }.Normalize();
}
