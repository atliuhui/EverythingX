using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Cn.Smart;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace EverythingX.Extensions
{
    // https://github.com/apache/lucenenet/blob/docs/4.8.0-beta00016/src/Lucene.Net.Analysis.SmartCn/SmartChineseAnalyzer.cs
    public sealed class ExtendSmartChineseAnalyzer : Analyzer
    {
        private readonly LuceneVersion matchVersion;
        private readonly CharArraySet stopWords;
        private readonly CharArraySet extendWords;
        private readonly SynonymMap synonyms;

        public ExtendSmartChineseAnalyzer(LuceneVersion matchVersion, CharArraySet stopWords, CharArraySet extendWords, SynonymMap synonyms)
        {
            this.matchVersion = matchVersion;
            this.stopWords = stopWords ?? CharArraySet.EMPTY_SET;
            this.extendWords = extendWords ?? CharArraySet.EMPTY_SET;
            this.synonyms = synonyms;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer;
            TokenStream result;
            if (matchVersion.OnOrAfter(LuceneVersion.LUCENE_48))
            {
                tokenizer = new HMMChineseTokenizer(reader);
                result = tokenizer;
            }
            else
            {
#pragma warning disable 612, 618
                tokenizer = new SentenceTokenizer(reader);
                result = new WordTokenFilter(tokenizer);
#pragma warning restore 612, 618
            }
            // result = new LowerCaseFilter(result);
            // LowerCaseFilter is not needed, as SegTokenFilter lowercases Basic Latin text.
            // The porter stemming is too strict, this is not a bug, this is a feature:)
            result = new PorterStemFilter(result);
            if (stopWords.Count > 0)
            {
                result = new StopFilter(matchVersion, result, stopWords);
            }
            if (extendWords.Any())
            {
                result = new ExtendWordFilter(result, extendWords);
            }
            if (synonyms != default)
            {
                result = new SynonymFilter(result, synonyms, true);
            }
            return new TokenStreamComponents(tokenizer, result);
        }
    }
}
