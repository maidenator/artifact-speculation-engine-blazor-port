using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("enka", client =>
{
    client.BaseAddress = new Uri("https://enka.network/");
    // Enka asks consumers to identify themselves.
    client.DefaultRequestHeaders.UserAgent.ParseAdd("ArtifactSpeculationBlazor/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Same-origin Enka proxy: browsers block direct calls because the Enka API
// sends no CORS headers. Server-side forwarding is not subject to CORS.
// The UID is validated here so a bad path can never reshape the upstream URL.
app.MapGet("/enka-api/uid/{uid}", async (string uid, IHttpClientFactory httpFactory) =>
{
    if (!Regex.IsMatch(uid, @"^\d{9,10}$"))
        return Results.BadRequest(new { error = "Invalid UID format." });

    using var client = httpFactory.CreateClient("enka");
    using var upstream = await client.GetAsync($"/api/uid/{uid}/");
    var body = await upstream.Content.ReadAsStringAsync();
    return Results.Content(body, "application/json", statusCode: (int)upstream.StatusCode);
});

app.MapFallbackToFile("index.html");

app.Run();
