using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Exceptions
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
