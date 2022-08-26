using System.Text;
using Lucene.Net.Search.Highlight;

namespace WhereIs;

public class ResultFormatter : IFormatter
{
    private const string DEFAULT_PRE = "**";

    private const string DEFAULT_POST = "**";

    internal string pre;

    internal string post;

    public ResultFormatter(
        string pre,
        string post)
    {
        this.pre = pre;
        this.post = post;
    }

    public ResultFormatter()
        : this(
              DEFAULT_PRE,
              DEFAULT_POST)
    {
    }

    public virtual string HighlightTerm(string originalText, TokenGroup tokenGroup)
    {
        if (tokenGroup.TotalScore <= 0f)
        {
            return originalText;
        }

        var stringBuilder = new StringBuilder(pre.Length + originalText.Length + post.Length);

        stringBuilder.Append(pre);
        stringBuilder.Append(originalText);
        stringBuilder.Append(post);

        return stringBuilder.ToString();
    }
}
