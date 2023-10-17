namespace NetTunnel.ClientAPI.Types
{
    /// <summary>
    /// The katzebase Case-insensitive lookup.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class InsensitiveDictionary<TValue> : Dictionary<string, TValue>
    {
        public InsensitiveDictionary() : base(StringComparer.InvariantCultureIgnoreCase) { }

        public InsensitiveDictionary<TValue> Clone()
        {
            var clone = new InsensitiveDictionary<TValue>();
            foreach (var source in this)
            {
                clone.Add(source.Key, source.Value);
            }
            return clone;
        }
    }
}
