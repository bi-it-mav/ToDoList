using System.Runtime.CompilerServices;

namespace ToDoList.Utils
{
    public sealed class ValueThenIdentityComparer<T> : IComparer<T>, IEquatable<ValueThenIdentityComparer<T>> where T : notnull
    {
        public static readonly ValueThenIdentityComparer<T> Instance = new();

        public int Compare(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            if (EqualityComparer<T>.Default.Equals(x, y)) return 0;
            return Comparer<T>.Default.Compare(x, y) switch
            {
                0 => RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y)),
                int ordering => ordering,
            };
        }

        public bool Equals(ValueThenIdentityComparer<T>? other)
            => other is not null;

        public override bool Equals(object? obj)
            => obj is ValueThenIdentityComparer<T>;

        public override int GetHashCode()
            => typeof(T).GetHashCode();

        public override string ToString()
            => $"{nameof(ValueThenIdentityComparer<T>)}<{typeof(T).Name}>";

        public static bool operator ==(ValueThenIdentityComparer<T>? x, ValueThenIdentityComparer<T>? y)
            => ReferenceEquals(x, y) || (x is not null && y is not null);

        public static bool operator !=(ValueThenIdentityComparer<T>? x, ValueThenIdentityComparer<T>? y)
            => !(x == y);
    }
}
