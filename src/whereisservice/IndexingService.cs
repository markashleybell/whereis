using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Core;

namespace WhereIsService;

internal class IndexingService
{
    private const LuceneVersion ApplicationLuceneVersion = LuceneVersion.LUCENE_48;

    private readonly IOptionsMonitor<Settings> _cfg;

    public IndexingService(
        IOptionsMonitor<Settings> optionsMonitor) =>
        _cfg = optionsMonitor;

    public void DeleteAndRebuildIndex(IEnumerable<File> documents)
    {
        if (_cfg.CurrentValue.SearchIndexBasePath is null)
        {
            return;
        }

        System.IO.Directory.CreateDirectory(_cfg.CurrentValue.SearchIndexBasePath);

        WithIndexWriter(writer => writer.AddDocuments(documents.Select(AsDocument)), overwriteIndex: true);
    }

    public void AddFile(File document) =>
        WithIndexWriter(writer => writer.AddDocument(AsDocument(document)));

    public void RemoveFile(File document) =>
        RemoveFile(document.Path);

    public void RemoveFile(string path) =>
        WithIndexWriter(writer => writer.DeleteDocuments(new Term("path", path)), applyDeletes: true);

    public void UpdateFile(File document) =>
        WithIndexWriter(writer => writer.UpdateDocument(new Term("path", document.Path), AsDocument(document)));

    private static Document AsDocument(File file) =>
        new() {
            new StringField("path", file.Path, Field.Store.YES),
            new TextField("name", file.Name, Field.Store.YES),
            new TextField("content", file.Content ?? string.Empty, Field.Store.YES),
            new TextField("created", DateTools.DateToString(file.Created, DateResolution.MILLISECOND), Field.Store.YES),
            new TextField("updated", DateTools.DateToString(file.Updated, DateResolution.MILLISECOND), Field.Store.YES),
        };

    private void WithIndexWriter(Action<IndexWriter> f, bool overwriteIndex = false, bool applyDeletes = false)
    {
        var analyzer = new SimpleAnalyzer(ApplicationLuceneVersion);

        var writerConfig = new IndexWriterConfig(ApplicationLuceneVersion, analyzer) {
            OpenMode = overwriteIndex ? OpenMode.CREATE : OpenMode.CREATE_OR_APPEND
        };

        using var dir = FSDirectory.Open(_cfg.CurrentValue.SearchIndexBasePath);

        using var writer = new IndexWriter(dir, writerConfig);

        f(writer);

        writer.Flush(triggerMerge: false, applyAllDeletes: applyDeletes);
    }
}
