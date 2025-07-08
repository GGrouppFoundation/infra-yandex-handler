using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PrimeFuncPack;

namespace GGroupp.Infra;

partial class SourceGeneratorExtensions
{
    internal static IncrementalValuesProvider<YandexHttpFunctonMetadata> GetHttpFunctonsProvider(
        this IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(IsActualType, GetHttpFunctions).SelectMany(InnerPipeSelf);

        static bool IsActualType(SyntaxNode syntaxNode, CancellationToken _)
        {
            if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
            {
                return false;
            }

            var modifiers = classDeclaration.Modifiers;
            return modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.InternalKeyword);
        }

        static YandexHttpFunctonMetadata[] InnerPipeSelf(YandexHttpFunctonMetadata[] source, CancellationToken _)
            =>
            source;
    }

    private static YandexHttpFunctonMetadata[] GetHttpFunctions(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol)
        {
            return [];
        }

        return typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(GetHttpFunction).FilterNotNull().ToArray();
    }

    private static YandexHttpFunctonMetadata? GetHttpFunction(IMethodSymbol method)
    {
        var functionAttribute = method.GetAttributes().FirstOrDefault(IsYandexHttpFunctonAttribute);
        if (functionAttribute is null)
        {
            return null;
        }

        var functionTypeName = functionAttribute.GetConstructorArgumentValue<string>(0) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(functionTypeName))
        {
            throw new InvalidOperationException("Yandex HTTP function name must be specified in the Attribute.");
        }

        if (method.IsStatic is false)
        {
            throw method.CreateInvalidMethodException("must be static");
        }

        if (method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw method.CreateInvalidMethodException("must be public or internal");
        }

        if (method.TypeParameters.Length > 0)
        {
            throw method.CreateInvalidMethodException("must have no generic parameters");
        }

        var resolvers = method.ReturnType.GetMembers().OfType<IMethodSymbol>().Select(GetResolveMethodHandlerType).FilterNotNull();
        if (resolvers.FirstOrDefault() is not YandexHandlerResolverMetadata resolver)
        {
            throw method.CreateInvalidMethodException("must return a type that contains Resolve method");
        }

        return new(
            dependencyProvider: new(
                typeData: method.ContainingType.GetDisplayedData(),
                resolver: resolver,
                methodName: method.Name,
                withContext: method.IsFunctionContextUsed()),
            functionTypeName: functionTypeName,
            serviceProvider: GetServiceProviderMetadata());

        YandexServiceProviderMetadata GetServiceProviderMetadata()
        {
            var builderType = functionAttribute.GetNamedArgumentValue<INamedTypeSymbol>("ProviderBuilderType")?.GetDisplayedData();
            var builderMethod = functionAttribute.GetNamedArgumentValue<string>("ProviderBuilderMethod") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(builderMethod))
            {
                if (builderType is null)
                {
                    return DefaultServiceProvider;
                }

                throw new InvalidOperationException(
                    $"ProviderBuilderMethod must be specified in the attribute for type {builderType.DisplayedTypeName}.");
            }

            return new(
                typeData: builderType ?? method.ContainingType.GetDisplayedData(),
                methodName: builderMethod);
        }
    }

    private static bool IsFunctionContextUsed(this IMethodSymbol method)
    {
        if (method.Parameters.Length is 0)
        {
            return false;
        }

        if (method.Parameters.Length > 1)
        {
            throw method.CreateInvalidMethodException("have invalid parameters length");
        }

        if (method.Parameters[0].Type is not INamedTypeSymbol type || type.IsType("Yandex.Cloud.Functions", "Context") is false)
        {
            throw method.CreateInvalidMethodException("have invalid parameter type");
        }

        return true;
    }
}