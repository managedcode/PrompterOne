namespace PrompterOne.Core.Models.Tps;

public class TpsPhrase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public int? PauseDuration { get; set; }
    public List<TpsWord> Words { get; set; } = new();
}
