using System.Linq;
using System.Text;
using PrimeFuncPack;

namespace GGroupp.Infra;

internal static partial class YandexHttpFunctonSourceBuilder
{
    private static SourceBuilder AppendServiceProviderInitializator(
        this SourceBuilder builder, YandexServiceProviderMetadata data)
        =>
        builder.AddUsing(
            data.TypeData.AllNamespaces.ToArray())
        .AppendCodeLine(
            $"LazyServiceProvider = new({data.TypeData.DisplayedTypeName}.{data.MethodName}, LazyThreadSafetyMode.ExecutionAndPublication);");

    private static SourceBuilder AppendRunnerLine(this SourceBuilder builder, YandexHandlerResolverMetadata resolver, string? lineStart = null)
    {
        var lineBuilder = new StringBuilder(lineStart).Append("await dependency.RunYandexHttpFunctonAsync");

        if (resolver.IsBaseInterface is false)
        {
            builder = builder
                .AddUsing(resolver.HandlerType.AllNamespaces.ToArray())
                .AddUsing(resolver.InputType.AllNamespaces.ToArray())
                .AddUsing(resolver.OutputType.AllNamespaces.ToArray());

            lineBuilder = lineBuilder
                .Append('<')
                .Append(resolver.HandlerType.DisplayedTypeName).Append(", ")
                .Append(resolver.InputType.DisplayedTypeName).Append(", ")
                .Append(resolver.OutputType.DisplayedTypeName)
                .Append('>');
        }

        return builder.AppendCodeLine(
            lineBuilder.Append('(').ToString())
        .BeginArguments()
        .AppendCodeLine(
            "LazyServiceProvider.Value, httpRequest);")
        .EndArguments();
    }
}