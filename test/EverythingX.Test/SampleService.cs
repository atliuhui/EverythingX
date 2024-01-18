using EverythingX.Services;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace EverythingX.Test
{
    internal class SampleService : IndexService
    {
        public SampleService(DirectoryInfo root)
            : base(root) { }

        public int Append()
        {
            var document = new Document();

            document.Add(new StringField(nameof(ThingModel.PathHash), "3a6dbcdbc26cb13e7aa09e0a2b60b4ab", StringField.Store.YES));

            return this.Append(document);
        }
        public void Search()
        {
            var query = new TermQuery(new Term(nameof(ThingModel.PathHash), "3a6dbcdbc26cb13e7aa09e0a2b60b4ab"));

            this.SearchWrap((searcher) =>
            {
                var hits = searcher.Search(query, 10);
                hits.ScoreDocs.ToList().ForEach(item =>
                {
                    Console.WriteLine(item.Doc);
                });
            });
        }
        public IEnumerable<string> Analyze(string text)
        {
            return this.AnalyzerWrap(analyzer =>
            {
                var words = new List<string>();
                using (var stream = analyzer.GetTokenStream("text", text))
                {
                    //var attribute = stream.AddAttribute<IOffsetAttribute>();
                    var attribute = stream.AddAttribute<ICharTermAttribute>();

                    stream.Reset();
                    while (stream.IncrementToken())
                    {
                        words.Add(attribute.ToString());
                    }
                    stream.End();
                }

                return words;
            });
        }

        public int Clear()
        {
            return this.Reset();
        }
    }
}
