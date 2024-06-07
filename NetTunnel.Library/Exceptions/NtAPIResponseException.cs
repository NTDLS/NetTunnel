using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Exceptions
{
    public class NtAPIResponseException : NtExceptionBase
    {
        public NtAPIResponseException()
        {
            Severity = NtLogSeverity.Warning;
        }

        public NtAPIResponseException(string? message)
            : base($"API exception: {message}.")
        {
            Severity = NtLogSeverity.Exception;
        }
    }
}