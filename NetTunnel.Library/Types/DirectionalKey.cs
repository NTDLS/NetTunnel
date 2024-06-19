using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class DirectionalKey : IEquatable<DirectionalKey>
    {
        public NtDirection Direction { get; private set; }
        public Guid Id { get; private set; }

        public DirectionalKey(Guid id, NtDirection direction)
        {
            Direction = direction;
            Id = id;
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
