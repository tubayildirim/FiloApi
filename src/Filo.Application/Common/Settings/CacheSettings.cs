namespace Filo.Application.Common.Settings;

public class CacheSettings
{
    public const string SectionName = "CacheSettings";
    public int DefaultExpirationMinutes { get; set; } = 10;
}
