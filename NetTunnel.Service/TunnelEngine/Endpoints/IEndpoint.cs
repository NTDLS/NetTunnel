namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    public interface IEndpoint
    {
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service endpoint has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }
        public bool KeepRunning { get; }
        public void Disconnect(Guid streamId);
        public void SendEndpointData(Guid streamId, byte[] buffer);
        public void Start();
        public void Stop();
    }
}
