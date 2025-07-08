using PrimeFuncPack;

namespace GGroupp.Infra;

internal sealed record class YandexDependencyProviderMetadata
{
    public YandexDependencyProviderMetadata(
        DisplayedTypeData typeData,
        YandexHandlerResolverMetadata resolver,
        string methodName,
        bool withContext)
    {
        TypeData = typeData;
        Resolver = resolver;
        MethodName = methodName ?? string.Empty;
        WithContext = withContext;
    }

    public DisplayedTypeData TypeData { get; }

    public YandexHandlerResolverMetadata Resolver { get; }

    public string MethodName { get; }

    public bool WithContext { get; }
}