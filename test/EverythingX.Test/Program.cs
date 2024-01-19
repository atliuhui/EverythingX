// See https://aka.ms/new-console-template for more information
using EverythingX.Extensions;
using EverythingX.Test;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
var sample = new SampleService(new DirectoryInfo(indices));

sample.Clear();
sample.Append();
sample.Search();

//var text = "交易中台架构设计：海量并发的高扩展，新业务秒级接入";
//var text = "快叫上你的蛋搭子一起进入蛋仔的世界吧！";
//var text = "蛋仔是蛋搭子的蛋仔";
var text = "蛋挞是蛋搭子的蛋挞";
//var text = @"日前，国家统计局发布2023年国民经济运行情况，全年国内生产总值比上年增长5.2%。这一年，我们顶住外部压力、克服内部困难，经济回升向好，供给需求稳步改善，转型升级持续推进，就业物价总体稳定，民生保障有力有效，高质量发展扎实推进，全面建设社会主义现代化国家迈出坚实步伐。这样的成绩难能可贵、殊为不易，为我们增强信心、稳定预期，推动中国经济长期持续健康发展奠定了坚实基础。";

var words = sample.Analyze(text);
var stats = words.OrderByDescending(item => item.Value).ToArray();
Console.WriteLine(string.Join(Environment.NewLine, stats));
// https://zhuanlan.zhihu.com/p/111775508

// https://github.com/apache/lucenenet/blob/master/src/Lucene.Net/Analysis/package.md
//var version = LuceneVersion.LUCENE_48;
//var stopWords = WordlistLoader.GetWordSet(File.OpenText(@"C:\Users\atliu\source\codespaces\EverythingX\src\EverythingX\stopwords.txt"), version);
//var extendWords = WordlistLoader.GetWordSet(File.OpenText(@"C:\Users\atliu\source\codespaces\EverythingX\src\EverythingX\extendwords.txt"), version);
//var synonyms = SynonymListLoader.GetSynonymMap(File.OpenText(@"C:\Users\atliu\source\codespaces\EverythingX\src\EverythingX\synonyms.txt"), version);

//using (var analyzer = new ExtendSmartChineseAnalyzer(version, stopWords, extendWords, synonyms))
//using (var stream = analyzer.GetTokenStream("field", text))
//{
//    //var attribute = stream.AddAttribute<IOffsetAttribute>();
//    var attribute = stream.AddAttribute<ICharTermAttribute>();

//    stream.Reset();
//    while (stream.IncrementToken())
//    {
//        Console.WriteLine(attribute);
//    }
//    stream.End();
//}
