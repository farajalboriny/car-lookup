namespace CarLookup.Core.Clients;

public sealed class NhtsaApiException : Exception
{
    public int? StatusCode { get; }

    public NhtsaApiException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
