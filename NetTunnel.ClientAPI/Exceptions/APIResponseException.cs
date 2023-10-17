using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class APIResponseException : ExceptionBase
    {
        public APIResponseException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public APIResponseException(string? message)
            : base($"API exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}