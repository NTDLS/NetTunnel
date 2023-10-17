using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NullException : ExceptionBase
    {
        public NullException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public NullException(string? message)
            : base($"Null exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}