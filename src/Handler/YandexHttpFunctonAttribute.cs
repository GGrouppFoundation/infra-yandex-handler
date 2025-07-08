using System;

namespace GGroupp.Infra;

[AttributeUsage(AttributeTargets.Method)]
public sealed class YandexHttpFunctonAttribute(string typeName) : Attribute
{
    public string TypeName { get; } = typeName ?? string.Empty;

    public Type? ProviderBuilderType { get; set; }

    public string? ProviderBuilderMethod { get; set; }
}