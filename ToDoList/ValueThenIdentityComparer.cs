using System.Runtime.CompilerServices;

namespace ToDoList
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
            return RuntimeHelpers.GetHashCode(x)
                .CompareTo(RuntimeHelpers.GetHashCode(y));
        }

        public bool Equals(ValueThenIdentityComparer<T>? other)
            => other is not null;

        public override bool Equals(object? obj)
            => obj is ValueThenIdentityComparer<T>;

        public override int GetHashCode()
            => typeof(T).GetHashCode();
    }
}
