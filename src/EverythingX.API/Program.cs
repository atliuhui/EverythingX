using CsvHelper;
using EverythingX.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/Analyze", async ([FromQuery(Name = "t")] string text) =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    //if (!things.Ready) return Results.NoContent();

    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear starting...");
    var result = things.Analyze(text);
    Console.WriteLine($"{DateTime.Now.ToString("s")}|Clear completed");

    return Results.Ok(result);
});
app.MapPost("/Index", async ([FromBody] IEnumerable<string> projections) =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    //if (!things.Ready) return Results.NoContent();

    int count = 0;
    foreach (var projection in projections)
    {
        var root = new DirectoryInfo(projection);
        var files = new FileService(root);

        things.Clear(root.FullName);

        files.ForEach(new string[] { ".txt", ".csv", ".tsv", ".lrc" }, (path, text) =>
        {
            things.AppendOrUpdate(new ThingInfo
            {
                Space = root.FullName,
                Path = path,
                Title = Path.GetFileNameWithoutExtension(path),
                Style = Path.GetExtension(path),
                Content = text,
            });
            count++;
        });
    }

    return Results.Ok(count);
});
app.MapGet("/Search", async ([FromQuery(Name = "k")] string keyword, [FromQuery] int take) =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return Results.NoContent();

    byte[] contents;
    using (var stream = new MemoryStream())
    using (var writer = new StreamWriter(stream, Encoding.UTF8))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        var result = things.Search(keyword.Trim(), take, 1);

        csv.WriteRecords(result.ScoreDocs.Select(item => new
        {
            item.Score,
            item.Title,
            item.Style,
            Fragment = item.ContentFragments.FirstOrDefault(),
        }));

        await csv.FlushAsync();
        contents = stream.ToArray();
    }

    return Results.File(contents, "text/csv");
});
app.MapDelete("/Reset", async () =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return Results.NoContent();

    var result = things.Clear();

    return Results.Ok(result);
});
app.MapGet("/Stat", async () =>
{
    var indices = Path.Combine(Environment.CurrentDirectory, @"assets\indices");
    var things = new ThingService(new DirectoryInfo(indices));

    if (!things.Ready) return Results.NoContent();

    var result = things.Stat();

    return Results.Ok(result);
});

app.Run();
