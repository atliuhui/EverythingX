using EverythingX.Extensions;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;

namespace EverythingX.Services
{
    public class ThingService : IndexService
    {
        public ThingService(DirectoryInfo root)
            : base(root) { }

        public new bool Ready => this.Ready();
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
        public ThingSearchResult Search(Query query, int takeDocuments = 20, int takeFragments = 5)
        {
            var result = new ThingSearchResult();

            this.SearchWrap((searcher) =>
            {
                var hits = searcher.Search(query, takeDocuments);
                result.MaxScore = hits.MaxScore;
                result.TotalHits = hits.TotalHits;
                result.ScoreDocs = hits.ScoreDocs.Select(item =>
                {
                    var doc = searcher.Doc(item.Doc);
                    return Convert(item, doc, query, takeFragments);
                }).ToArray();
            });

            return result;
        }
        public ThingSearchResult Search(Dictionary<string, string> patterns, int takeDocuments = 20, int takeFragments = 5)
        {
            var terms = new MultiPhraseQuery();

            foreach (var item in patterns)
            {
                terms.Add(new Term(item.Key, item.Value));
            }

            return this.Search(terms, takeDocuments, takeFragments);
        }
        public ThingSearchResult Search(string keyword, int takeDocuments = 20, int takeFragments = 5)
        {
            return this.AnalyzerWrap(analyzer =>
            {
                var parser = new QueryParser(AppLuceneVersion, nameof(ThingInfo.Content), analyzer);
                return this.Search(parser.Parse(keyword), takeDocuments, takeFragments);
            });
        }
        public ThingModel Find(int id)
        {
            var result = new ThingModel();

            this.SearchWrap((searcher) =>
            {
                var doc = searcher.Doc(id);
                result = Convert(doc);
            });

            return result;
        }
        public bool Exists(string path)
        {
            try
            {
                var result = this.Search(new Dictionary<string, string>
                {
                    [nameof(ThingModel.PathHash)] = StringExtension.CreateHash(path),
                }, 1);

                return result.ScoreDocs.Any();
            }
            catch (IndexNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int AppendOrUpdate(ThingInfo info)
        {
            var model = Convert(info);
            var document = Convert(model);

            if (this.Exists(info.Path))
            {
                return this.Update(new Term(nameof(ThingModel.PathHash), model.PathHash), document);
            }
            else
            {
                return this.Append(document);
            }
        }
        public int Delete(string path)
        {
            if (this.Exists(path))
            {
                return this.Delete(new Term(nameof(ThingModel.PathHash), StringExtension.CreateHash(path)));
            }

            return 0;
        }
        public int Clear(string space)
        {
            return this.Delete(new Term(nameof(ThingModel.SpaceHash), StringExtension.CreateHash(space)));
        }
        public int Clear()
        {
            return this.Reset();
        }
        public ThingStatResult Stat()
        {
            var result = new ThingStatResult();

            this.SearchWrap((searcher) =>
            {
                result.Count = searcher.IndexReader.NumDocs;
            });

            result.Sizes = this.root.GetFiles("*", SearchOption.AllDirectories).Sum(item => item.Length);

            return result;
        }

        ThingModel Convert(ThingInfo info)
        {
            return new ThingModel
            {
                SpaceHash = StringExtension.CreateHash(info.Space),
                Space = info.Space,
                PathHash = StringExtension.CreateHash(info.Path),
                Path = info.Path,
                Title = info.Title,
                Style = info.Style,
                ContentHash = StringExtension.CreateHash(info.Content),
                Content = info.Content,
            };
        }
        Document Convert(ThingModel model)
        {
            // https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
            // https://northcoder.com/post/lucene-fields-and-term-vectors/
            var document = new Document();
            document.Add(new StringField(nameof(ThingModel.SpaceHash), model.SpaceHash, Field.Store.YES));
            document.Add(new StringField(nameof(ThingModel.Space), model.Space, Field.Store.YES));
            document.Add(new StringField(nameof(ThingModel.PathHash), model.PathHash, Field.Store.YES));
            document.Add(new StringField(nameof(ThingModel.Path), model.Path, Field.Store.YES));
            document.Add(new TextField(nameof(ThingModel.Title), model.Title, Field.Store.YES));
            document.Add(new StringField(nameof(ThingModel.Style), model.Style, Field.Store.YES));
            document.Add(new StringField(nameof(ThingModel.ContentHash), model.ContentHash, Field.Store.YES));
            document.Add(new TextField(nameof(ThingModel.Content), model.Content, Field.Store.YES));
            return document;
        }
        ThingModel Convert(Document doc)
        {
            return new ThingModel
            {
                SpaceHash = doc.Get(nameof(ThingSearchResultItem.SpaceHash)),
                Space = doc.Get(nameof(ThingSearchResultItem.Space)),
                PathHash = doc.Get(nameof(ThingSearchResultItem.PathHash)),
                Path = doc.Get(nameof(ThingSearchResultItem.Path)),
                Title = doc.Get(nameof(ThingSearchResultItem.Title)),
                Style = doc.Get(nameof(ThingSearchResultItem.Style)),
                ContentHash = doc.Get(nameof(ThingSearchResultItem.ContentHash)),
                Content = doc.Get(nameof(ThingSearchResultItem.Content)),
            };
        }
        ThingSearchResultItem Convert(ScoreDoc scoreDoc, Document doc, Query query, int takeFragments = 5)
        {
            var item = Convert(doc);

            return new ThingSearchResultItem
            {
                SpaceHash = item.SpaceHash,
                Space = item.Space,
                PathHash = item.PathHash,
                Path = item.Path,
                Title = item.Title,
                Style = item.Style,
                ContentHash = item.ContentHash,
                Content = item.Content,
                Score = scoreDoc.Score,
                ContentFragments = HighlightFragments(item.Content, query, doc, takeFragments),
            };
        }
        IEnumerable<string> HighlightFragments(string content, Query query, Document doc, int take = 5)
        {
            return this.AnalyzerWrap(analyzer =>
            {
                var htmlFormatter = new SimpleHTMLFormatter();
                var highlighter = new Highlighter(htmlFormatter, new QueryScorer(query));
                var tokenStream = TokenSources.GetTokenStream(doc, nameof(ThingSearchResultItem.Content), analyzer);
                var fragments = highlighter.GetBestTextFragments(tokenStream, content, mergeContiguousFragments: true, maxNumFragments: take);
                return fragments.Where(item => item != null && item.Score > 0).Select(item => item.ToString());
            });
        }
    }

    public class ThingInfo
    {
        public string Space { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Style { get; set; }
        public string Content { get; set; }
    }
    public class ThingModel : ThingInfo
    {
        public string SpaceHash { get; set; }
        public string PathHash { get; set; }
        public string ContentHash { get; set; }
    }
    public class ThingSearchResultItem : ThingModel
    {
        public float Score { get; set; }
        public IEnumerable<string> ContentFragments { get; set; }
    }
    public class ThingSearchResult
    {
        public float MaxScore { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<ThingSearchResultItem> ScoreDocs { get; set; }
    }
    public class ThingStatResult
    {
        public int Count { get; set; }
        public long Sizes { get; set; }
    }
}
