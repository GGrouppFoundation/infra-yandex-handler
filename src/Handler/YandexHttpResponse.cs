using System.Collections.Generic;

namespace GGroupp.Infra;

public sealed record class YandexHttpResponse
{
    public int StatusCode { get; init; }

    public Dictionary<string, string>? Headers { get; init; }

    public string? Body { get; init; }
}