using System.Globalization;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace WhereIs;

internal static class SearchFunctions
{
    private const LuceneVersion ApplicationLuceneVersion = LuceneVersion.LUCENE_48;

    public static IEnumerable<SearchResult> Search(string query)
    {
        using var dir = FSDirectory.Open(@"D:\Temp\whereis\index");

        using IndexReader reader = DirectoryReader.Open(dir);

        var searcher = new IndexSearcher(reader);

        var analyzer = new SimpleAnalyzer(ApplicationLuceneVersion);

        var boosts = new Dictionary<string, float> {
            ["path"] = 0.8f,
            ["name"] = 1.5f,
            ["content"] = 1.0f
        };

        var parser = new MultiFieldQueryParser(ApplicationLuceneVersion, new[] { "path", "name", "content" }, analyzer, boosts);

        var luceneQuery = parser.Parse(query);

        var hits = searcher.Search(luceneQuery, n: 20).ScoreDocs;

        var formatter = new ResultFormatter();
        var highlighter = new Highlighter(formatter, new QueryScorer(luceneQuery));

        var results = hits.Select(h => {
            var doc = searcher.Doc(h.Doc);

            var content = doc.Get("content");
            var tokenStream = TokenSources.GetAnyTokenStream(searcher.IndexReader, h.Doc, "content", analyzer);
            var fragments = highlighter.GetBestTextFragments(tokenStream, content, true, 10);

            return new SearchResult(
                h.Score,
                doc.Get("path"),
                doc.Get("name"),
                content,
                DateTime.TryParse(doc.Get("created"), null, DateTimeStyles.None, out var c) ? c : DateTime.MinValue,
                DateTime.TryParse(doc.Get("updated"), null, DateTimeStyles.None, out var u) ? u : DateTime.MinValue,
                fragments.Select(f => f.ToString()).ToArray()
            );
        });

        // IMPORTANT: ToList materialises the query results; if we don't do this we get an ObjectDisposedException
        return results
            .OrderByDescending(r => r.Score)
            .ToList();
    }
}
