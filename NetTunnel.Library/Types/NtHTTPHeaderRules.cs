namespace NetTunnel.Library
{
    public class NtHTTPHeaderRules
    {
        public List<NtHttpHeaderRule> Collection = new();

        public void Add(NtHttpHeaderRule rule)
        {
            Collection.Add(rule);
        }
    }
}