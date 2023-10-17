using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtAPIResponseException : NtExceptionBase
    {
        public NtAPIResponseException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public NtAPIResponseException(string? message)
            : base($"API exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}