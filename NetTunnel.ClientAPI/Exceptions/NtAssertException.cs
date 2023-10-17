using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtAssertException : NtExceptionBase
    {
        public NtAssertException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public NtAssertException(string? message)
            : base($"Assert exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}