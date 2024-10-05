namespace NDiscoPlus.Code.LightHandlers;

internal class InvalidLightHandlerConfigException : Exception
{
    public InvalidLightHandlerConfigException() : base()
    {
    }

    public InvalidLightHandlerConfigException(string? message) : base(message)
    {
    }

    public InvalidLightHandlerConfigException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}