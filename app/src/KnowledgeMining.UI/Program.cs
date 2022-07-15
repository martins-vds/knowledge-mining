using Azure.Identity;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.Infrastructure.Jobs;
using KnowledgeMining.UI.Api;
using KnowledgeMining.UI.Extensions;
using KnowledgeMining.UI.Services.Links;
using KnowledgeMining.UI.Services.Search;
using KnowledgeMining.UI.Services.Search.Models;
using KnowledgeMining.UI.Services.Storage;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Azure;
using MudBlazor.Services;
using System.Threading.Channels;

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

            builder.Services.Configure<AzureMapsOptions>(builder.Configuration.GetSection(AzureMapsOptions.AzureMaps));
            builder.Services.Configure<CustomizationsOptions>(builder.Configuration.GetSection(CustomizationsOptions.Customizations));
            builder.Services.Configure<EntityMapOptions>(builder.Configuration.GetSection(EntityMapOptions.EntityMap));
            builder.Services.Configure<SearchOptions>(builder.Configuration.GetSection(SearchOptions.Search));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.Storage));
            builder.Services.Configure<AzureSignalROptions>(builder.Configuration.GetSection(AzureSignalROptions.SignalR));

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.ConfigureDefaults(builder.Configuration.GetSection("AzureDefaults"));
                clientBuilder.UseCredential(new DefaultAzureCredential());

                clientBuilder.AddBlobServiceClient(builder.Configuration.GetSection(StorageOptions.Storage));
                clientBuilder.AddSearchClient(builder.Configuration.GetSection(SearchOptions.Search));
                clientBuilder.AddSearchIndexClient(builder.Configuration.GetSection(SearchOptions.Search));
                clientBuilder.AddSearchIndexerClient(builder.Configuration.GetSection(SearchOptions.Search));
            });

            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IStorageService, StorageService>();
            builder.Services.AddScoped<ILinkGenerator, DocumentPreviewLinkGenerator>();

            builder.Services.AddSingleton(Channel.CreateUnbounded<SearchIndexerJobContext>(new UnboundedChannelOptions() { SingleWriter = true, SingleReader = true }));
            builder.Services.AddSingleton(provider =>
            {
                return provider.GetService<Channel<SearchIndexerJobContext>>().Writer;
            });
            builder.Services.AddSingleton(provider =>
            {
                return provider.GetService<Channel<SearchIndexerJobContext>>().Reader;
            });
            builder.Services.AddHostedService<SearchIndexerJob>();

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