using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using OpenTelemetry.Metrics;

namespace WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = true)
            .AddViewLocalization()
            .AddDataAnnotationsLocalization()
            .AddViewOptions(options => options.HtmlHelperOptions.ClientValidationEnabled = false);
        builder.Services.Configure<WebEncoderOptions>(options =>
        {
            options.TextEncoderSettings = new(UnicodeRanges.All);
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.AppendTrailingSlash = false;
            options.LowercaseQueryStrings = false;
        });
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation();
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddPrometheusExporter();
            });
        builder.Services.AddHealthChecks();
        var app = builder.Build();
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapGet("/api/todo", () => Results.Ok(new Todo()
        {
            Id = 42,
            IsComplete = true,
            Name = "Name of todo"
        }));
        app.MapDefaultControllerRoute();
        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = static _ => false
        });
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = static _ => false
        });
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        app.Run();
    }
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}