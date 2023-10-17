using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtNullException : NtExceptionBase
    {
        public NtNullException()
        {
            Severity = NtLogSeverity.Warning;
        }

        public NtNullException(string? message)
            : base($"Null exception: {message}.")

        {
            Severity = NtLogSeverity.Exception;
        }
    }
}