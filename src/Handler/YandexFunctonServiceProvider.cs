using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra;

public static class YandexFunctonServiceProvider
{
    public static ServiceProvider Build()
        =>
        BuildServiceProvider(default, default);

    public static ServiceProvider BuildServiceProvider(
        Action<IServiceCollection>? configureServices,
        Action<ILoggingBuilder>? configureLogger)
    {
        var services = new ServiceCollection()
            .AddLogging(InnerConfigureLogger)
            .AddSingleton(BuildConfiguration)
            .AddSocketsHttpHandlerProviderAsSingleton();

        configureServices?.Invoke(services);

        return services.BuildServiceProvider();

        void InnerConfigureLogger(ILoggingBuilder builder)
        {
            builder = builder.AddConsole();
            configureLogger?.Invoke(builder);
        }
    }

    private static IConfiguration BuildConfiguration(IServiceProvider _)
        =>
        new ConfigurationBuilder().AddEnvironmentVariables().Build();
}