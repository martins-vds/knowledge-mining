using Azure.Identity;
using KnowledgeMining.UI.Extensions;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Search;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace KnowledgeMining.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<AzureMapsOptions>(Configuration.GetSection(AzureMapsOptions.AzureMaps));
            services.Configure<CustomizationsOptions>(Configuration.GetSection(CustomizationsOptions.Customizations));
            services.Configure<GraphOptions>(Configuration.GetSection(GraphOptions.Graph));
            services.Configure<SearchOptions>(Configuration.GetSection(SearchOptions.Search));
            services.Configure<StorageOptions>(Configuration.GetSection(StorageOptions.Storage));

            services.AddAzureClients(builder =>
            {
                builder.ConfigureDefaults(Configuration.GetSection("AzureDefaults"));
                builder.UseCredential(new DefaultAzureCredential());

                builder.AddBlobServiceClient(Configuration.GetSection(StorageOptions.Storage));
                builder.AddSearchClient(Configuration.GetSection(SearchOptions.Search));
                builder.AddSearchIndexClient(Configuration.GetSection(SearchOptions.Search));
                builder.AddSearchIndexerClient(Configuration.GetSection(SearchOptions.Search));
            });

            services.AddScoped<ISearchService, SearchService>();
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            services.AddApplicationInsightsTelemetry();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
