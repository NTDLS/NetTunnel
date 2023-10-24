namespace NetTunnel.Service.Types
{
    public interface IEndpoint
    {
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service endpoint has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }
    }
}
