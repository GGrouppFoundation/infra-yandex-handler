namespace GGroupp.Infra;

internal sealed record class YandexHttpFunctonMetadata
{
    public YandexHttpFunctonMetadata(
        YandexDependencyProviderMetadata dependencyProvider,
        string functionTypeName,
        YandexServiceProviderMetadata serviceProvider)
    {
        DependencyProvider = dependencyProvider;
        FunctionTypeName = functionTypeName ?? string.Empty;
        ServiceProvider = serviceProvider;
    }

    public YandexDependencyProviderMetadata DependencyProvider { get; }

    public string FunctionTypeName { get; }

    public YandexServiceProviderMetadata ServiceProvider { get; }
}