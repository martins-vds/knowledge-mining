using Azure.Identity;
using KnowledgeMining.UI.Extensions;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Search;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;

namespace KnowledgeMining.Blazor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddMudServices();

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Services.Configure<AzureMapsOptions>(builder.Configuration.GetSection(AzureMapsOptions.AzureMaps));
            builder.Services.Configure<CustomizationsOptions>(builder.Configuration.GetSection(CustomizationsOptions.Customizations));
            builder.Services.Configure<GraphOptions>(builder.Configuration.GetSection(GraphOptions.Graph));
            builder.Services.Configure<SearchOptions>(builder.Configuration.GetSection(SearchOptions.Search));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.Storage));

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
            builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddServiceProfiler();

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

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}