using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class HttpHeaderRule
    {
        /// <summary>
        /// Inbound, outbound, etc.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtHttpHeaderType HeaderType { get; set; }

        /// <summary>
        /// Insert, update, delete, etc.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtHttpHeaderAction Action { get; set; }
        /// <summary>

        /// Get, put, post, etc.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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

        public HttpHeaderRule(NtHttpVerb verb, string name, NtHttpHeaderAction action, string value)
        {
            Verb = verb;
            Name = name;
            Action = action;
            Value = value;
        }

        public HttpHeaderRule()
        {
        }

        public HttpHeaderRule CloneConfiguration()
        {
            return new HttpHeaderRule
            {
                Name = Name,
                Value = Value,
                Description = Description,
                Action = Action,
                Enabled = Enabled,
                HeaderType = HeaderType,
                Verb = Verb
            };
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                + Enabled.GetHashCode()
                + Verb.GetHashCode()
                + Action.GetHashCode()
                + Value.GetHashCode()
                + Description.GetHashCode();
        }
    }
}
