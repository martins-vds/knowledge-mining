using FluentValidation;
using KnowledgeMining.Application.Common.Behaviours;
using KnowledgeMining.Application.Common.Options;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.Configure<AzureMapsOptions>(configuration.GetSection(AzureMapsOptions.AzureMaps));
            services.Configure<CustomizationsOptions>(configuration.GetSection(CustomizationsOptions.Customizations));
            services.Configure<EntityMapOptions>(configuration.GetSection(EntityMapOptions.EntityMap));
            services.Configure<SearchOptions>(configuration.GetSection(SearchOptions.Search));
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.Storage));
            services.Configure<AzureSignalROptions>(configuration.GetSection(AzureSignalROptions.SignalR));

            return services;
        }
    }
}
