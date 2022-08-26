namespace WhereIs;

public class SearchResult
{
    public SearchResult(
        float score,
        string name,
        string path,
        string? content,
        DateTime created,
        DateTime updated,
        string[] highlights)
    {
        Score = score;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Content = content;
        Created = created;
        Updated = updated;
        Highlights = highlights ?? throw new ArgumentNullException(nameof(highlights));
    }

    public float Score { get; }

    public string Name { get; }

    public string Path { get; }

    public string? Content { get; }

    public DateTime Created { get; }

    public DateTime Updated { get; }

    public string[] Highlights { get; }
}
