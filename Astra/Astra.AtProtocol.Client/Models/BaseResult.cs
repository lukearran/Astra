namespace Astra.AtProtocol.Client.Models;

public abstract class BaseResult(bool success, string message)
{
    public bool Success { get; } = success;
    public string Message { get; } = message;
}