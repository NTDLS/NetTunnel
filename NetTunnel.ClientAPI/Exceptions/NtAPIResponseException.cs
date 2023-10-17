using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
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