namespace WhereIsService;

public class File
{
    public File(
        string name,
        string path,
        string? content,
        DateTime created,
        DateTime updated)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Content = content;
        Created = created;
        Updated = updated;
    }

    public string Name { get; }

    public string Path { get; }

    public string? Content { get; }

    public DateTime Created { get; }

    public DateTime Updated { get; }

    public static File From(FileInfo fileInfo) =>
        new(
            fileInfo.Name,
            fileInfo.FullName,
            System.IO.File.ReadAllText(fileInfo.FullName),
            fileInfo.CreationTime,
            fileInfo.LastWriteTime);
}
