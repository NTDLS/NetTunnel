using NetTunnel.Library.Interfaces;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class DirectionalKey : IEquatable<DirectionalKey>
    {
        public NtDirection Direction { get; set; } = NtDirection.Undefined;
        public Guid Id { get; set; } = Guid.Empty;

        public DirectionalKey()
        {
        }

        public DirectionalKey(Guid id, NtDirection direction)
        {
            Direction = direction;
            Id = id;
        }

        public DirectionalKey(ITunnel tunnel)
        {
            Direction = tunnel.Direction;
            Id = tunnel.Configuration.TunnelId;
        }

        public DirectionalKey(IEndpoint tunnel)
        {
            Direction = tunnel.Direction;
            Id = tunnel.Configuration.EndpointId;
        }

        public override string ToString() => $"{Id}:{Direction}";

        public override bool Equals(object? obj)
        {
            return Equals(obj as DirectionalKey);
        }

        public bool Equals(DirectionalKey? other)
        {
            return ToString() == other?.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(DirectionalKey? left, DirectionalKey? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(DirectionalKey? left, DirectionalKey? right)
        {
            return !(left == right);
        }
    }
}
