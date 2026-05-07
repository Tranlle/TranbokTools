using System.Text;

namespace TOrbit.Plugin.Promptor.Models;

public sealed class PromptorLogEntry
{
    public required DateTime Time { get; init; }
    public required string StrategyLabel { get; init; }
    public required string Model { get; init; }
    public required string Provider { get; init; }
    public required string Endpoint { get; init; }
    public required bool IsSuccess { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int MaxTokens { get; init; }
    public required double Temperature { get; init; }
    public required string InputPreview { get; init; }
    public required string Input { get; init; }
    public string? Output { get; init; }
    public string? ErrorMessage { get; init; }
    public string StatusText => IsSuccess ? "Success" : "Failed";
    public string TimeText => Time.ToString("yyyy-MM-dd HH:mm:ss");
    public string DurationText => $"{Duration.TotalSeconds:F2}s";
    public string ModelText => $"{Model} ({Provider})";
    public string ParameterText => $"Max {MaxTokens}, Temp {Temperature:F2}";
    public string OutputPreview => ToPreview(Output);
    public string ErrorPreview => ToPreview(ErrorMessage);

    public string FormatAsText()
    {
        const string sep = "--------------------------------------------------------";
        var sb = new StringBuilder();
        sb.AppendLine(sep);
        sb.AppendLine($"  Time      {Time:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"  Strategy  {StrategyLabel}");
        sb.AppendLine($"  Model     {Model} ({Provider})");
        sb.AppendLine($"  Endpoint  {Endpoint}");
        sb.AppendLine($"  Params    MaxTokens={MaxTokens}, Temperature={Temperature:F2}");
        sb.AppendLine($"  Duration  {Duration.TotalSeconds:F2}s");
        sb.AppendLine($"  Status    {(IsSuccess ? "Success" : "Failed")}");
        if (!string.IsNullOrEmpty(ErrorMessage))
            sb.AppendLine($"  Error     {ErrorMessage}");
        if (!string.IsNullOrEmpty(InputPreview))
            sb.AppendLine($"  Input     {InputPreview}");
        if (!string.IsNullOrEmpty(Output))
            sb.AppendLine($"  Output    {OutputPreview}");
        sb.Append(sep);
        return sb.ToString();
    }

    private static string ToPreview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 140 ? normalized : normalized[..140] + "...";
    }
}
