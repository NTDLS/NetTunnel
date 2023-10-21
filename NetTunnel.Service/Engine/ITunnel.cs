namespace NetTunnel.Service.Engine
{
    public interface ITunnel
    {
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service tunnel has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }
    }
}
