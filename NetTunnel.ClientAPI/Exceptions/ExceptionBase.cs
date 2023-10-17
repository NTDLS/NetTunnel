using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class ExceptionBase : Exception
    {
        public KbLogSeverity Severity { get; set; }

        public ExceptionBase()
        {
            Severity = KbLogSeverity.Exception;
        }

        public ExceptionBase(string? message)
            : base(message)

        {
            Severity = KbLogSeverity.Exception;
        }
    }
}
