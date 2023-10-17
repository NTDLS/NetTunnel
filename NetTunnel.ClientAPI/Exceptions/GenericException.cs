using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class GenericException : ExceptionBase
    {
        public GenericException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public GenericException(string? message)
            : base($"Generic exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}