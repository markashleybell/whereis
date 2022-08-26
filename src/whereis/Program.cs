using Lucene.Net.Search.Highlight;
using WhereIs;

var results = SearchFunctions.Search(args[0]);

var indent = "    ";

foreach (var result in results)
{
    Console.WriteLine(result.Name);

    foreach (var highlight in result.Highlights)
    {
        Console.WriteLine(indent + highlight);
    }
}
