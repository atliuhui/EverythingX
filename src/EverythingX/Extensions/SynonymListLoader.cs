using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Util;

namespace EverythingX.Extensions
{
    public class SynonymListLoader
    {
        private const string SYNONYM_FILE_COMMENT = "//";
        private const string SYNONYM_LINE_COMMENT = "|";
        private const string SYNONYM_WORD_PRIMARY = "=";
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

                    var primary = line.Split(SYNONYM_WORD_PRIMARY).ElementAt(0).Trim();
                    var secondary = line.Split(SYNONYM_WORD_PRIMARY).ElementAt(1).Trim();
                    var words = secondary.Split(SYNONYM_WORD_SECONDARY).Select(item => item.Trim()).ToArray();
                    builder.Add(new CharsRef(primary), SynonymMap.Builder.Join(words, new CharsRef()), true);
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
