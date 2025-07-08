using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GarageGroup.Infra;
using PrimeFuncPack;

namespace GGroupp.Infra;

public static class YandexFunctonDependency
{
    public static Task<YandexHttpResponse> RunYandexHttpFunctonAsync<TIn, TOut>(
        this Dependency<IHandler<TIn, TOut>> dependency, IServiceProvider serviceProvider, [AllowNull] string httpRequest)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return dependency.Resolve(serviceProvider).RunYandexHttpFunctonAsync(serviceProvider, httpRequest);
    }

    public static Task<YandexHttpResponse> RunYandexHttpFunctonAsync<THandler, TIn, TOut>(
        this Dependency<THandler> dependency, IServiceProvider serviceProvider, [AllowNull] string httpRequest)
        where THandler : IHandler<TIn, TOut>
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return dependency.Resolve(serviceProvider).RunYandexHttpFunctonAsync(serviceProvider, httpRequest);
    }
}