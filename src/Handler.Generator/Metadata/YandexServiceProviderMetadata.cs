using PrimeFuncPack;

namespace GGroupp.Infra;

internal sealed record class YandexServiceProviderMetadata
{
    public YandexServiceProviderMetadata(DisplayedTypeData typeData, string methodName)
    {
        TypeData = typeData;
        MethodName = methodName ?? string.Empty;
    }

    public DisplayedTypeData TypeData { get; }

    public string MethodName { get; }
}