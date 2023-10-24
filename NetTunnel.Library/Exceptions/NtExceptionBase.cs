using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Exceptions
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
