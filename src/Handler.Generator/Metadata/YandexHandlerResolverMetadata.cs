using PrimeFuncPack;

namespace GGroupp.Infra;

internal sealed record class YandexHandlerResolverMetadata
{
    public YandexHandlerResolverMetadata(
        DisplayedTypeData typeData,
        string methodName,
        DisplayedTypeData handlerType,
        DisplayedTypeData inputType,
        DisplayedTypeData outputType,
        bool isBaseInterface)
    {
        TypeData = typeData;
        MethodName = methodName ?? string.Empty;
        HandlerType = handlerType;
        InputType = inputType;
        OutputType = outputType;
        IsBaseInterface = isBaseInterface;
    }

    public DisplayedTypeData TypeData { get; }

    public string MethodName { get; }

    public DisplayedTypeData HandlerType { get; }

    public DisplayedTypeData InputType { get; }

    public DisplayedTypeData OutputType { get; }

    public bool IsBaseInterface { get; }
}