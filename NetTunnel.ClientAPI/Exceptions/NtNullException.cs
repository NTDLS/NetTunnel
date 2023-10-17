using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtNullException : NtExceptionBase
    {
        public NtNullException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public NtNullException(string? message)
            : base($"Null exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}