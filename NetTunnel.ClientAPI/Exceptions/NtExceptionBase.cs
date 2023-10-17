using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtExceptionBase : Exception
    {
        public KbLogSeverity Severity { get; set; }

        public NtExceptionBase()
        {
            Severity = KbLogSeverity.Exception;
        }

        public NtExceptionBase(string? message)
            : base(message)

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}
