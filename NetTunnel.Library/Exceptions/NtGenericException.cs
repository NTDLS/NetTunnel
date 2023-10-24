using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Exceptions
{
    public class NtGenericException : NtExceptionBase
    {
        public NtGenericException()
        {
            Severity = NtLogSeverity.Warning;
        }

        public NtGenericException(string? message)
            : base($"Generic exception: {message}.")

        {
            Severity = NtLogSeverity.Exception;
        }
    }
}