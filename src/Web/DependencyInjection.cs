using System.Reflection;
using MongoDB.Driver;
using Quartz;
using Quartz.AspNetCore;
using StackExchange.Redis;
using Web.Data;
using Web.Jobs;
using FluentValidation;
using MediatR;
using Web.Common.Models.Options;
using Web.Services.Implementations;
using Web.Services.Interfaces;
using Web.UseCases;

namespace Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModels(configuration)
            .AddMongoDb()
            .AddFluentValidation()
            .AddCacheServices()
            .AddQuartzJob()
            .AddUseCases();
        return services;
    }

    private static IServiceCollection AddModels(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettingModel = configuration.GetSection("Settings").Get<AppSettingModel>();
        ArgumentNullException.ThrowIfNull(appSettingModel);
        services.AddSingleton(appSettingModel);
        return services;
    }

    private static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        var settings = services.BuildServiceProvider().GetRequiredService<AppSettingModel>();
        ArgumentNullException.ThrowIfNull(settings);

        var mongoClient = new MongoClient(settings.MongoDb.ConnectionString);
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton<MongoDbContext>();

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

    private static IServiceCollection AddCacheServices(this IServiceCollection services)
    {
        var settings = services.BuildServiceProvider().GetRequiredService<AppSettingModel>();
        ArgumentNullException.ThrowIfNull(settings);

        var redisOptions = ConfigurationOptions.Parse($"{settings.Redis.Host}:{settings.Redis.Port}");
        redisOptions.Password = settings.Redis.Password;
        redisOptions.AbortOnConnectFail = false;
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    private static IServiceCollection AddQuartzJob(this IServiceCollection services)
    {
        var settings = services.BuildServiceProvider().GetRequiredService<AppSettingModel>();
        ArgumentNullException.ThrowIfNull(settings);

        services.AddQuartz(quartz =>
        {
            quartz.SchedulerName = "UrlShortenScheduler";
            quartz.MisfireThreshold = TimeSpan.FromSeconds(300);

            var tokenSeedJobKey = new JobKey(nameof(TokenSeedJob));
            quartz.AddJob<TokenSeedJob>(tokenSeedJobKey, job => job.WithDescription("Token Seed Job"));
            quartz.AddTrigger(trigger => trigger
                .WithIdentity("TokenSeedJobTrigger")
                .ForJob(tokenSeedJobKey)
                .WithCronSchedule("0 0/5 * * * ?")
                .WithDescription("Token Seed Job Trigger"));
        });
        services.AddQuartzServer(opt => opt.WaitForJobsToComplete = false);
        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
}