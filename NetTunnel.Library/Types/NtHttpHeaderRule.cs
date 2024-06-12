using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class NtHttpHeaderRule
    {
        public NtHttpHeaderAction Action { get; set; }
        public NtHttpVerb Verb { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public NtHttpHeaderRule( NtHttpVerb verb, string name, NtHttpHeaderAction action, string value)
        {
            Verb = verb;
            Name = name;
            Action = action;
            Value = value;
        }

        public NtHttpHeaderRule() { }
    }
}
