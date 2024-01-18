// See https://aka.ms/new-console-template for more information
using CommandLine;
using EverythingX.Services;

Parser.Default.ParseArguments<AnalyzeOption, IndexOption, FindOption, SearchOption, ResetOption, StatOption>(args)
.WithParsed((Action<AnalyzeOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    //if (!things.Ready) return;

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear starting...");
    var result = things.Analyze(option.Text);
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear completed");

    Console.WriteLine(string.Join('|', result));
}))
.WithParsed((Action<IndexOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    //if (!things.Ready) return;

    var root = new DirectoryInfo(option.Path);
    var files = new FileService(root);

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear starting...");
    things.Clear(root.FullName);
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear completed");

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Index starting...");
    files.ForEach(new string[] { ".txt", ".csv", ".tsv", ".lrc" }, (path, text) =>
    {
        var count = things.AppendOrUpdate(new ThingInfo
        {
            Space = root.FullName,
            Path = path,
            Title = Path.GetFileNameWithoutExtension(path),
            Style = Path.GetExtension(path),
            Content = text,
        });
        Console.Write($"\r{string.Empty.PadRight(Console.WindowWidth)}");
        Console.Write($"\r{Path.GetFileName(path)}");
    });
    Console.Write($"\r{string.Empty.PadRight(Console.WindowWidth)}");
    Console.WriteLine($"\r{DateTime.Now.ToString("s")}|Index completed");
}))
.WithParsed((Action<FindOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return;

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Search starting...");
    var result = things.Find(option.Id);
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Search completed");

    Console.WriteLine($"{nameof(result.SpaceHash)}   :{result.SpaceHash}");
    Console.WriteLine($"{nameof(result.Space)}       :{result.Space}");
    Console.WriteLine($"{nameof(result.PathHash)}    :{result.PathHash}");
    Console.WriteLine($"{nameof(result.Path)}        :{result.Path}");
    Console.WriteLine($"{nameof(result.Title)}       :{result.Title}");
    Console.WriteLine($"{nameof(result.Style)}       :{result.Style}");
    Console.WriteLine($"{nameof(result.ContentHash)} :{result.ContentHash}");
    Console.WriteLine($"{nameof(result.Content)}     :");
    Console.WriteLine($"{result.Content}");
}))
.WithParsed((Action<SearchOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return;

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Search starting...");
    var result = things.Search(option.Keyword.Trim(), option.Take, 1);
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Search completed");

    foreach (var item in result.ScoreDocs)
    {
        Console.WriteLine($"{item.Title}{item.Style},{item.ContentFragments.FirstOrDefault()?.Replace("\r\n", string.Empty)}");
        //Console.WriteLine($"{item.PathHash},{item.Score}");
    }
}))
.WithParsed((Action<ResetOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return;

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Reset starting...");
    things.Clear();
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Reset completed");
}))
.WithParsed((Action<StatOption>)(option =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return;

    var result = things.Stat();
    Console.WriteLine($"{nameof(result.Count)}   :{result.Count}");
    Console.WriteLine($"{nameof(result.Sizes)}   :{result.Sizes.ToString("N0")}");
}))
.WithNotParsed(errors => { });

#if DEBUG
[Verb("Analyze", true, HelpText = "Analyze")]
#else
[Verb("Analyze", false, HelpText = "Analyze")]
#endif
internal class AnalyzeOption
{
#if DEBUG
    [Option('t', "Text", Required = false, HelpText = "Text", Default = @"蛋仔是蛋搭子的蛋仔")]
#else
    [Option('t', "Text", Required = true, HelpText = "Text")]
#endif
    public string Text { get; set; }
}

#if DEBUG
[Verb("Index", false, HelpText = "Index")]
#else
[Verb("Index", false, HelpText = "Index")]
#endif
internal class IndexOption
{
#if DEBUG
    [Option('p', "Path", Required = false, HelpText = "Path", Default = @"C:\Users\atliu\Desktop\playlists\Lyrics")]
#else
    [Option('p', "Path", Required = true, HelpText = "Path")]
#endif
    public string Path { get; set; }
}

#if DEBUG
[Verb("Find", false, HelpText = "Find")]
#else
[Verb("Find", false, HelpText = "Find")]
#endif
internal class FindOption
{
#if DEBUG
    [Option("Id", Required = false, HelpText = "Id", Default = 1)]
#else
    [Option("Id", Required = true, HelpText = "Id")]
#endif
    public int Id { get; set; }
}

#if DEBUG
[Verb("Search", false, HelpText = "Search")]
#else
[Verb("Search", true, HelpText = "Search")]
#endif
internal class SearchOption
{
#if DEBUG
    [Option('k', "Keyword", Required = false, HelpText = "Keyword", Default = "看起来缺少可惜遗憾")]
#else
    [Option('k', "Keyword", Required = true, HelpText = "Keyword")]
#endif
    public string Keyword { get; set; }
    [Option("Take", Required = false, HelpText = "Take", Default = 10)]
    public int Take { get; set; }
}

#if DEBUG
[Verb("Reset", false, HelpText = "Reset")]
#else
[Verb("Reset", false, HelpText = "Reset")]
#endif
internal class ResetOption
{
}

#if DEBUG
[Verb("Stat", false, HelpText = "Stat")]
#else
[Verb("Stat", false, HelpText = "Stat")]
#endif
internal class StatOption
{
}
