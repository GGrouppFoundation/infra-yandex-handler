using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using GarageGroup.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra;

internal static class YandexFunctonHandlerRunner
{
    private const int StatusCodeOk = 200;

    private const int StatusCodeNoContent = 204;

    private const int StatusCodeBadRequest = 400;

    private const int StatusCodeInternalServerError = 500;

    private const string ContentTypeHeaderName = "Content-Type";

    private static readonly JsonSerializerOptions SerializerOptions
        =
        new(JsonSerializerDefaults.Web);

    public static async Task<YandexHttpResponse> RunYandexHttpFunctonAsync<TIn, TOut>(
        this IHandler<TIn, TOut> handler, [AllowNull] IServiceProvider serviceProvider, [AllowNull] string httpRequest)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await DeserializeOrFailure<TIn>(httpRequest).ForwardValueAsync(handler.InnerInvokeAsync);
        return result.Fold(MapSuccess, MapFailure);

        static YandexHttpResponse MapSuccess(TOut success)
        {
            if (success is Unit || success is null)
            {
                return new()
                {
                    StatusCode = StatusCodeNoContent
                };
            }

            if (success is string text)
            {
                return new()
                {
                    StatusCode = StatusCodeOk,
                    Body = text
                };
            }

            return new()
            {
                StatusCode = StatusCodeOk,
                Headers = new()
                {
                    [ContentTypeHeaderName] = MediaTypeNames.Application.Json
                },
                Body = JsonSerializer.Serialize(success, SerializerOptions)
            };
        }

        YandexHttpResponse MapFailure(Failure<HandlerFailureCode> failure)
        {
            int statusCode;
            var logger = serviceProvider?.GetLogger<TIn, TOut>();

            if (failure.FailureCode is HandlerFailureCode.Persistent)
            {
                logger?.LogError(failure.SourceException, "Persistent Yandex Functions Handler HTTP error: {error}", failure.FailureMessage);
                statusCode = StatusCodeBadRequest;
            }
            else
            {
                logger?.LogError(failure.SourceException, "Transient Yandex Functions Handler HTTP error: {error}", failure.FailureMessage);
                statusCode = StatusCodeInternalServerError;
            }

            return new()
            {
                StatusCode = statusCode,
                Body = failure.FailureMessage
            };
        }
    }

    private static async ValueTask<Result<TOut, Failure<HandlerFailureCode>>> InnerInvokeAsync<TIn, TOut>(
        this IHandler<TIn, TOut> handler, TIn? input)
    {
        try
        {
            return await handler.HandleAsync(input, default).ConfigureAwait(false); ;
        }
        finally
        {
            if (handler is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private static Result<TIn?, Failure<HandlerFailureCode>> DeserializeOrFailure<TIn>(string? httpRequest)
    {
        if (string.IsNullOrEmpty(httpRequest))
        {
            return Result.Success<TIn?>(default);
        }

        try
        {
            var request = JsonSerializer.Deserialize<YandexHttpRequest>(httpRequest, SerializerOptions);
            if (string.IsNullOrEmpty(request.Body))
            {
                return Result.Success<TIn?>(default);
            }

            if (request.IsBase64Encoded is false)
            {
                return JsonSerializer.Deserialize<TIn>(request.Body, SerializerOptions);
            }

            var requestBody = Convert.FromBase64String(request.Body);
            return JsonSerializer.Deserialize<TIn>(requestBody, SerializerOptions);
        }
        catch (Exception ex)
        {
            return ex.ToFailure(
                failureCode: HandlerFailureCode.Transient,
                failureMessage: "An unexpected excepton was thrown trying to deserialize Yandex Function Handler input.");
        }
    }

    private static ILogger<InnerType<TIn, TOut>>? GetLogger<TIn, TOut>(this IServiceProvider serviceProvider)
        =>
        serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<InnerType<TIn, TOut>>();

    private sealed class InnerType<TIn, TOut>;
}