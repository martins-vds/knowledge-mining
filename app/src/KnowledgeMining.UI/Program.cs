using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.UI.Api;
using KnowledgeMining.UI.Services.Links;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Azure;
using MudBlazor.Services;

namespace KnowledgeMining.UI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.CaptureStartupErrors(true);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddMudServices();

            builder.Services.AddSignalR().AddAzureSignalR(options =>
            {
                options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddScoped<ILinkGenerator, DocumentPreviewLinkGenerator>();

            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();

            app.MapGet(PreviewFileEndpoint.Route, async (
                    string fileName,
                    IStorageService storageClient,
                    CancellationToken cancellationToken) => await PreviewFileEndpoint.DownloadInlineFile(fileName, storageClient, cancellationToken))
               .WithName(PreviewFileEndpoint.EndpointName);

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}