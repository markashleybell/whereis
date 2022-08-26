namespace WhereIsService;

public class Settings
{
    public string? SearchIndexBasePath { get; set; }

    public string[] WatchFolders { get; set; }
        = Array.Empty<string>();
}
