namespace ToDoList.Utils
{
    public sealed class Atomic<T>(T value) where T : class
    {
        private T _value = value;

        public T Value => Volatile.Read(ref _value);

        public T Update(Func<T, T> mutator)
        {
            T initRead = Volatile.Read(ref _value);
            T updated;
            bool successful;
            do
            {
                updated = mutator(initRead);
                if (object.ReferenceEquals(initRead, updated))
                {
                    return updated;
                }
                T checkRead = Interlocked.CompareExchange(ref _value, updated, initRead);
                successful = object.ReferenceEquals(initRead, checkRead);
                initRead = checkRead; // We already have a volatile read that we can reuse for the next loop
            }
            while (!successful);
            return updated;
        }
    }
}
