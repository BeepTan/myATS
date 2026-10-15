using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ATS.Blazor.Components;
using ATS.Blazor.Data;
using ATS.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Configure SQLite Database with WAL mode and Busy Timeout
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ats.db";
builder.Services.AddDbContext<AtsDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 2. Register Typed HttpClient for Python FastAPI with Polly Resilience Pipeline
var aiBaseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
builder.Services.AddHttpClient<FastAiClientService>(client =>
{
    client.BaseAddress = new Uri(aiBaseUrl);
    client.Timeout = Timeout.InfiniteTimeSpan; // Managed by Polly resilience pipeline
})
.AddStandardResilienceHandler(options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
    options.Retry.MaxRetryAttempts = 2;
    // Sampling duration must be at least double of AttemptTimeout (60s >= 2 * 20s)
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
});

// 4. Register Resilient AI Parser Service (Decorates FastAiClient with local fallback)
builder.Services.AddScoped<IAiParserService, ResilientAiParserService>();

// 5. Register Background Evaluation Queue
builder.Services.AddSingleton<EvaluationQueueService>();
builder.Services.AddSingleton<IEvaluationQueue>(sp => sp.GetRequiredService<EvaluationQueueService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<EvaluationQueueService>());

// 6. Register Role State Service (Circuit-scoped role access control)
builder.Services.AddScoped<RoleStateService>();

var app = builder.Build();

// Ensure SQLite database & seed data are initialized
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AtsDbContext>();
    db.Database.EnsureCreated();
    
    // Enable SQLite Write-Ahead Logging (WAL) for high concurrency
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000;");
}

// Ensure resume upload storage folder exists
var uploadsFolder = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "resumes");
if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

// Minimal API endpoint for securely downloading candidate resumes
app.MapGet("/api/resumes/{applicationId:guid}/download", async (Guid applicationId, AtsDbContext db) =>
{
    var appRecord = await db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
    if (appRecord == null || string.IsNullOrEmpty(appRecord.ResumeFilePath) || !File.Exists(appRecord.ResumeFilePath))
    {
        return Results.NotFound("Resume file not found.");
    }

    var contentType = appRecord.ResumeFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
        ? "application/pdf"
        : "image/jpeg";

    return Results.File(appRecord.ResumeFilePath, contentType, appRecord.ResumeFileName);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
