using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    /// <summary>
    /// The endpoint contains information that defines all we need to listen or connect to an endpoint.
    /// They are OWNED by the tunnel configuration and at the server that created the tunnel.
    /// These are sent to the tunnel service when the tunnel is connected, but once the connection
    ///     is made - they can be altered at either end by the service UI.
    /// </summary>
    public class EndpointConfiguration
    {
        public Guid EndpointId { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }

        /// <summary>
        /// The type of traffic accepted at the inbound endpoint. Https terminates TLS using the certificate below.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType InboundTrafficType { get; set; } = NtTrafficType.Raw;

        /// <summary>
        /// The type of traffic sent from the outbound endpoint. Https re-encrypts the connection to the destination.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType OutboundTrafficType { get; set; } = NtTrafficType.Raw;

        /// <summary>
        /// The traffic type that applies to the end of the endpoint described by <see cref="Direction"/>.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public NtTrafficType TrafficType => Direction == NtDirection.Outbound ? OutboundTrafficType : InboundTrafficType;

        public List<HttpHeaderRule> HttpHeaderRules { get; set; } = new();

        /// <summary>
        /// Path to the PFX/PKCS#12 certificate used to terminate TLS at the inbound endpoint (HTTPS traffic type only).
        /// </summary>
        public string TlsCertificatePath { get; set; } = string.Empty;
        /// <summary>
        /// Password for the PFX certificate (if any).
        /// </summary>
        public string TlsCertificatePassword { get; set; } = string.Empty;
        /// <summary>
        /// When true, the outbound endpoint will not validate the backend server's certificate when re-encrypting (HTTPS traffic type only).
        /// </summary>
        public bool TlsIgnoreBackendCertificateErrors { get; set; }

        public DirectionalKey EndpointKey => new(EndpointId, Direction);

        public EndpointConfiguration SwapDirection()
        {
            var clone = CloneConfiguration();
            clone.Direction = Constants.SwapDirection(clone.Direction);
            return clone;
        }

        public EndpointConfiguration()
        {
        }

        public EndpointConfiguration(Guid endpointId, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<HttpHeaderRule> httpHeaderRules, NtTrafficType inboundTrafficType, NtTrafficType outboundTrafficType)
        {
            EndpointId = endpointId;
            Direction = NtDirection.Undefined;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            InboundTrafficType = inboundTrafficType;
            OutboundTrafficType = outboundTrafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public EndpointConfiguration(Guid endpointId, NtDirection direction, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<HttpHeaderRule> httpHeaderRules, NtTrafficType inboundTrafficType, NtTrafficType outboundTrafficType)
        {
            EndpointId = endpointId;
            Direction = direction;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            InboundTrafficType = inboundTrafficType;
            OutboundTrafficType = outboundTrafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public EndpointConfiguration CloneConfiguration()
        {
            var clone = new EndpointConfiguration
            {
                EndpointId = EndpointId,
                Direction = Direction,
                Name = Name,
                OutboundAddress = OutboundAddress,
                InboundPort = InboundPort,
                OutboundPort = OutboundPort,
                InboundTrafficType = InboundTrafficType,
                OutboundTrafficType = OutboundTrafficType,
                TlsCertificatePath = TlsCertificatePath,
                TlsCertificatePassword = TlsCertificatePassword,
                TlsIgnoreBackendCertificateErrors = TlsIgnoreBackendCertificateErrors,
            };

            foreach (var rule in HttpHeaderRules)
            {
                clone.HttpHeaderRules.Add(rule.CloneConfiguration());
            }

            return clone;
        }

        public override int GetHashCode()
        {
            return Utility.CombineHashes([EndpointId.GetHashCode(),
                Name.GetHashCode(),
                Direction.GetHashCode(),
                OutboundAddress.GetHashCode(),
                InboundPort.GetHashCode(),
                OutboundPort.GetHashCode(),
                InboundTrafficType.GetHashCode(),
                OutboundTrafficType.GetHashCode(),
                TlsCertificatePath.GetHashCode(),
                TlsCertificatePassword.GetHashCode(),
                TlsIgnoreBackendCertificateErrors.GetHashCode(),
                HttpHeaderRules.GetHashCode()]);
        }
    }
}
