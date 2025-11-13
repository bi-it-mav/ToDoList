using System.Collections.Immutable;

namespace ToDoList.Services
{
    public class StateChangeNotifier<TTopic> where TTopic : notnull
    {
        private static readonly ImmutableSortedSet<Func<Task>> EmptyTopicSubscriptions = ImmutableSortedSet<Func<Task>>.Empty
            .WithComparer(HashCodeComparer<Func<Task>>());
        private ImmutableDictionary<TTopic, ImmutableSortedSet<Func<Task>>> _subscriptions = ImmutableDictionary<TTopic, ImmutableSortedSet<Func<Task>>>.Empty;


        public async Task<IDisposable> SubscribeAsync(TTopic topic, Func<Task> updateState) => await SubscribeAsync([topic], updateState);
        public async Task<IDisposable> SubscribeAsync(IImmutableSet<TTopic> topics, Func<Task> updateState)
        {
            var initTask = updateState.Invoke();

            foreach (var topic in topics)
            {
                ImmutableInterlocked.AddOrUpdate(
                    ref _subscriptions,
                    topic,
                    EmptyTopicSubscriptions.Add(updateState),
                    (topic, topicSubscriptions) => topicSubscriptions.Add(updateState)
                );
            }

            await initTask;

            return new Unsubscriber(() =>
            {
                foreach (var topic in topics)
                {
                    ImmutableInterlocked.AddOrUpdate(
                        ref _subscriptions,
                        topic,
                        EmptyTopicSubscriptions,    
                        (topic, topicSubscriptions) => topicSubscriptions.Remove(updateState)
                    );
                }
            });
        }

        public async Task NotifyAsync(TTopic topic) => await NotifyAsync([topic]);
        public async Task NotifyAsync(IImmutableSet<TTopic> topics)
        {
            var upadateFunctions = topics
                .ToImmutableSortedSet(HashCodeComparer<TTopic>())
                .SelectMany(topic => _subscriptions.GetValueOrDefault(topic, EmptyTopicSubscriptions))
                .ToImmutableSortedSet();
            var upadateTasks = upadateFunctions
                .Select(updateState => updateState())
                .ToArray();
            await Task.WhenAll(upadateTasks);
        }

        private class Unsubscriber(Action unsubscribe) : IDisposable
        {
            private readonly Action _unsubscribe = unsubscribe;

            public void Dispose() => _unsubscribe();
        }

        private static Comparer<T> HashCodeComparer<T>() => Comparer<T>.Create(static (a, b) => a.GetHashCode().CompareTo(b.GetHashCode()));
    }
}
