using System.Linq;
using PrimeFuncPack;

namespace GGroupp.Infra;

partial class YandexHttpFunctonSourceBuilder
{
    internal static string BuildSourceCode(this YandexHttpFunctonMetadata metadata)
        =>
        new SourceBuilder(
            metadata.DependencyProvider.TypeData.AllNamespaces.FirstOrDefault())
        .AddUsing(
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "GGroupp.Infra",
            "Yandex.Cloud.Functions")
        .AppendCodeLine(
            $"internal sealed class {metadata.FunctionTypeName} : YcFunction<string?, Task<YandexHttpResponse>>")
        .BeginCodeBlock()
        .AppendReadonlyFields(
            metadata)
        .AppendEmptyLine()
        .AppendStaticConstructor(
            metadata)
        .AppendEmptyLine()
        .AppendFunctionHandler(
            metadata)
        .EndCodeBlock()
        .Build();

    private static SourceBuilder AppendReadonlyFields(this SourceBuilder builder, YandexHttpFunctonMetadata metadata)
    {
        builder = builder.AppendCodeLine("private static readonly Lazy<IServiceProvider> LazyServiceProvider;");
        if (metadata.DependencyProvider.WithContext)
        {
            return builder;
        }

        return builder.AppendEmptyLine()
            .AddUsing(metadata.DependencyProvider.Resolver.TypeData.AllNamespaces.ToArray())
            .AppendCodeLine($"private static readonly {metadata.DependencyProvider.Resolver.TypeData.DisplayedTypeName} dependency;");
    }

    private static SourceBuilder AppendStaticConstructor(this SourceBuilder builder, YandexHttpFunctonMetadata metadata)
    {
        builder = builder.AppendCodeLine($"static {metadata.FunctionTypeName}()");
        if (metadata.DependencyProvider.WithContext)
        {
            return builder.BeginLambda().AppendServiceProviderInitializator(metadata.ServiceProvider).EndLambda();
        }

        var dependencyProvider = metadata.DependencyProvider;

        return builder.BeginCodeBlock()
            .AppendServiceProviderInitializator(metadata.ServiceProvider)
            .AddUsing(metadata.DependencyProvider.TypeData.AllNamespaces.ToArray())
            .AppendCodeLine($"dependency = {dependencyProvider.TypeData.DisplayedTypeName}.{dependencyProvider.MethodName}();")
            .EndCodeBlock();
    }

    private static SourceBuilder AppendFunctionHandler(this SourceBuilder builder, YandexHttpFunctonMetadata metadata)
    {
        var dependencyProvider = metadata.DependencyProvider;
        var contextParameterName = dependencyProvider.WithContext ? "context" : "_";

        builder = builder.AppendCodeLine(
            $"public async Task<YandexHttpResponse> FunctionHandler(string? httpRequest, Context {contextParameterName})");

        if (dependencyProvider.WithContext)
        {
            return builder.BeginCodeBlock()
            .AppendCodeLine(
                $"ArgumentNullException.ThrowIfNull({contextParameterName});")
            .AppendEmptyLine()
            .AppendCodeLine(
                $"var dependency = {dependencyProvider.TypeData.DisplayedTypeName}.{dependencyProvider.MethodName}({contextParameterName});")
            .AppendRunnerLine(
                dependencyProvider.Resolver, "return ")
            .EndCodeBlock();
        }

        return builder.BeginLambda().AppendRunnerLine(dependencyProvider.Resolver).EndLambda();
    }
}