using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class NtHttpHeaderRule
    {
        /// <summary>
        /// Inbound, outbound, etc.
        /// </summary>
        public NtHttpHeaderType HeaderType { get; set; }

        /// <summary>
        /// Insert, update, delete, etc.
        /// </summary>
        public NtHttpHeaderAction Action { get; set; }
        /// <summary>

        /// Get, put, post, etc.
        /// </summary>
        public NtHttpVerb Verb { get; set; }

        /// <summary>
        /// Whether to apply the http header rule or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The name of the http header value.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// The value of the http header.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// User friendly description of the http header rule.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        public NtHttpHeaderRule(NtHttpVerb verb, string name, NtHttpHeaderAction action, string value)
        {
            Verb = verb;
            Name = name;
            Action = action;
            Value = value;
        }

        public NtHttpHeaderRule() { }
    }
}
