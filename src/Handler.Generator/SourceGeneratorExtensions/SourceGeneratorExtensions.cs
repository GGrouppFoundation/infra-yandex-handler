using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PrimeFuncPack;

namespace GGroupp.Infra;

internal static partial class SourceGeneratorExtensions
{
    private const string InfraYandexNamespace = "GGroupp.Infra";

    private const string InfraDefaultNamespace = "GarageGroup.Infra";

    private static readonly YandexServiceProviderMetadata DefaultServiceProvider
        =
        new(
            typeData: new([InfraYandexNamespace], "YandexFunctonServiceProvider"),
            methodName: "Build");

    private static bool IsYandexHttpFunctonAttribute(AttributeData attributeData)
        =>
        attributeData.AttributeClass?.IsType(InfraYandexNamespace, "YandexHttpFunctonAttribute") is true;

    private static IEnumerable<T> FilterNotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item is null)
            {
                continue;
            }

            yield return item!;
        }
    }

    private static YandexHandlerResolverMetadata? GetResolveMethodHandlerType(IMethodSymbol method)
    {
        if (method.IsStatic || method.IsGenericMethod || method.DeclaredAccessibility is not Accessibility.Public)
        {
            return null;
        }

        if (string.Equals("Resolve", method.Name, StringComparison.InvariantCulture) is false)
        {
            return null;
        }

        if (method.Parameters.Length is not 1)
        {
            return null;
        }

        if (method.Parameters[0].Type is not INamedTypeSymbol methodType)
        {
            return null;
        }

        if (methodType.IsType("System", "IServiceProvider") is false)
        {
            return null;
        }

        if (method.ReturnType is not INamedTypeSymbol returnType)
        {
            return null;
        }

        if (IsHandlerType(returnType))
        {
            return CreateMetadata(returnType, true);
        }

        if (returnType.AllInterfaces.Where(IsHandlerType).FirstOrDefault() is not INamedTypeSymbol handlerInterface)
        {
            return null;
        }

        return CreateMetadata(handlerInterface, false);

        static bool IsHandlerType(INamedTypeSymbol? typeSymbol)
            =>
            typeSymbol?.IsType(InfraDefaultNamespace, "IHandler") is true && typeSymbol.TypeArguments.Length is 2;

        YandexHandlerResolverMetadata CreateMetadata(INamedTypeSymbol type, bool isBaseInterface)
            =>
            new(
                typeData: method.ContainingType.GetDisplayedData(),
                methodName: method.Name,
                handlerType: returnType.GetDisplayedData(),
                inputType: type.TypeArguments[0].GetDisplayedData(),
                outputType: type.TypeArguments[1].GetDisplayedData(),
                isBaseInterface: isBaseInterface);
    }

    private static InvalidOperationException CreateInvalidMethodException(this IMethodSymbol resolverMethod, string message)
        =>
        new($"Yandex HTTP handler resolver method {resolverMethod.ContainingType?.Name}.{resolverMethod.Name} {message}.");
}