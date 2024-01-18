using EverythingX.Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace EverythingX.Services
{
    public class IndexService
    {
        public const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        protected readonly DirectoryInfo root;

        static void CreateIfNotExists(DirectoryInfo root)
        {
            if (!root.Exists)
            {
                root.Create();
            }
        }
        public IndexService(DirectoryInfo root)
        {
            this.root = root;

            CreateIfNotExists(this.root);
        }

        protected bool Ready()
        {
            try
            {
                using (var directory = FSDirectory.Open(this.root))
                using (var reader = DirectoryReader.Open(directory))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        protected T AnalyzerWrap<T>(Func<Analyzer, T> action)
        {
            var stopWords = WordlistLoader.GetWordSet(File.OpenText("stopwords.txt"), AppLuceneVersion);
            var extendWords = WordlistLoader.GetWordSet(File.OpenText("extendwords.txt"), AppLuceneVersion);
            var synonyms = SynonymListLoader.GetSynonymMap(File.OpenText("synonyms.txt"), AppLuceneVersion);

            // https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-smartcn/Lucene.Net.Analysis.Cn.Smart.html
            using (var analyzer = new ExtendSmartChineseAnalyzer(AppLuceneVersion, stopWords, extendWords, synonyms))
            {
                return action(analyzer);
            }
        }
        protected int WriteWrap(Action<IndexWriter, Analyzer> action)
        {
            return AnalyzerWrap(analyzer =>
            {
                var config = new IndexWriterConfig(AppLuceneVersion, analyzer);

                using (var directory = FSDirectory.Open(this.root))
                using (var writer = new IndexWriter(directory, config))
                {
                    action(writer, analyzer);
                    var count = writer.NumDocs;
                    writer.Commit();

                    return count;
                }
            });
        }
        protected void SearchWrap(Action<IndexSearcher> action)
        {
            using (var directory = FSDirectory.Open(this.root))
            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);
                action(searcher);
            }
        }

        protected int Append(Document document)
        {
            return this.WriteWrap((writer, analyzer) =>
            {
                writer.AddDocument(document, analyzer);
            });
        }
        protected int Update(Term term, Document document)
        {
            return this.WriteWrap((writer, analyzer) =>
            {
                writer.UpdateDocument(term, document, analyzer);
            });
        }
        protected int Delete(Term term)
        {
            return this.WriteWrap((writer, analyzer) =>
            {
                writer.DeleteDocuments(term);
            });
        }
        protected int Reset()
        {
            return this.WriteWrap((writer, analyzer) =>
            {
                writer.DeleteAll();
            });
        }
    }
}
