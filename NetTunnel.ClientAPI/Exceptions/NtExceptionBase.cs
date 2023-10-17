using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtExceptionBase : Exception
    {
        public NtLogSeverity Severity { get; set; }

        public NtExceptionBase()
        {
            Severity = NtLogSeverity.Exception;
        }

        public NtExceptionBase(string? message)
            : base(message)

        {
            Severity = NtLogSeverity.Exception;
        }
    }
}
