using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Util;

namespace EverythingX.Extensions
{
    public class SynonymListLoader
    {
        private const string SYNONYM_FILE_COMMENT = "//";
        private const string SYNONYM_LINE_COMMENT = "|";
        private const string SYNONYM_WORD_SECONDARY = ",";

        public static SynonymMap GetSynonymMap(TextReader reader, LuceneVersion matchVersion)
        {
            try
            {
                // https://github.com/apache/lucenenet/blob/docs/4.8.0-beta00016/src/Lucene.Net.Tests.Analysis.Common/Analysis/Miscellaneous/TestLimitTokenPositionFilter.cs
                var builder = new SynonymMap.Builder(true);

                string? line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) { continue; }
                    if (line.StartsWith(SYNONYM_FILE_COMMENT)) { continue; }

                    line = line.Trim();
                    int comment = line.IndexOf(SYNONYM_LINE_COMMENT);
                    if (comment >= 0)
                    {
                        line = line.Substring(0, comment).Trim();
                    }

                    var words = line.Split(SYNONYM_WORD_SECONDARY).Select(item => item.Trim()).Distinct().ToArray();
                    foreach (var word in words)
                    {
                        var diffs = words.Except(new string[] { word }).ToArray();
                        builder.Add(new CharsRef(word), SynonymMap.Builder.Join(diffs, new CharsRef()), true);
                    }
                }

                return builder.Build();
            }
            finally
            {
                IOUtils.Dispose(reader);
            }
        }
    }
}
