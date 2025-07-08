using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GGroupp.Infra;

[Generator(LanguageNames.CSharp)]
public sealed class YandexHttpFunctonSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.GetHttpFunctonsProvider();
        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, YandexHttpFunctonMetadata metadata)
    {
        var sourceCode = metadata.BuildSourceCode();
        context.AddSource($"{metadata.FunctionTypeName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
    }
}