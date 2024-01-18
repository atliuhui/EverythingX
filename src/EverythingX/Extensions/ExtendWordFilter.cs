using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace EverythingX.Extensions
{
    // https://zhuanlan.zhihu.com/p/143200104
    public sealed class ExtendWordFilter : TokenFilter
    {
        int matchedWordLength = 0;
        readonly ICharTermAttribute termAtt;
        readonly IPositionIncrementAttribute posIncAtt;

        readonly IEnumerable<string> extendWords;

        public ExtendWordFilter(TokenStream input, IEnumerable<string> extendWords)
            : base(input)
        {
            this.termAtt = this.AddAttribute<ICharTermAttribute>();
            this.posIncAtt = this.AddAttribute<IPositionIncrementAttribute>();
            this.extendWords = extendWords;
        }

        public override bool IncrementToken()
        {
            int skippedPositions = 0;

            while (this.m_input.IncrementToken())
            {
                if (Contains())
                {
                    if (skippedPositions != 0)
                    {
                        posIncAtt.PositionIncrement = posIncAtt.PositionIncrement + skippedPositions;
                    }
                    return true;
                }
                skippedPositions += posIncAtt.PositionIncrement;
            }

            return false;
        }

        bool Contains()
        {
            var word = this.extendWords.FirstOrDefault(item =>
            {
                return item.Contains(termAtt?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            });
            if (word != null)
            {
                matchedWordLength += termAtt.Length;

                if (matchedWordLength == word.Length)
                {
                    termAtt.SetEmpty();
                    termAtt.Append(word);
                    return true;
                }
            }
            else
            {
                matchedWordLength = 0;
            }

            return string.IsNullOrEmpty(word);
        }
    }
}
