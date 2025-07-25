namespace GGroupp.Infra;

public readonly record struct YandexHttpRequest
{
    public string? Body { get; init; }

    public bool IsBase64Encoded { get; init; }
}