using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class AssertException : ExceptionBase
    {
        public AssertException()
        {
            Severity = KbLogSeverity.Warning;
        }

        public AssertException(string? message)
            : base($"Assert exception: {message}.")

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}