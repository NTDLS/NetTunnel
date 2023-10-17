﻿using static NetTunnel.ClientAPI.Constants;

namespace NetTunnel.ClientAPI.Exceptions
{
    public class NtAssertException : NtExceptionBase
    {
        public NtAssertException()
        {
            Severity = NtLogSeverity.Warning;
        }

        public NtAssertException(string? message)
            : base($"Assert exception: {message}.")

        {
            Severity = NtLogSeverity.Exception;
        }
    }
}