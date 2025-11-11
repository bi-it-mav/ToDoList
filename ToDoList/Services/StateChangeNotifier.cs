using System;
using System.Collections.Immutable;

namespace ToDoList.Services
{
    public class StateChangeNotifier
    {
        private static readonly ImmutableSortedSet<Func<Task>> EmptyTopicSubscriptions = ImmutableSortedSet<Func<Task>>.Empty
            .WithComparer(Comparer<Func<Task>>.Create(static (a, b) => a.GetHashCode().CompareTo(b.GetHashCode())));
        private ImmutableDictionary<Type, ImmutableSortedSet<Func<Task>>> _subscriptions = ImmutableDictionary<Type, ImmutableSortedSet<Func<Task>>>.Empty;

        public async Task<IDisposable> SubscribeAsync(Type topic, Func<Task> updateState)
        {
            var initTask = updateState.Invoke();

            ImmutableInterlocked.AddOrUpdate(
                ref _subscriptions,
                topic,
                EmptyTopicSubscriptions.Add(updateState),
                (topic, topicSubscriptions) => topicSubscriptions.Add(updateState)
            );

            await initTask;

            return new Unsubscriber(() =>
            {
                ImmutableInterlocked.AddOrUpdate(
                    ref _subscriptions,
                    topic,
                    EmptyTopicSubscriptions,
                    (topic, topicSubscriptions) => topicSubscriptions.Remove(updateState)
                );
            });
        }
        public async Task NotifyAsync(Type key)
        {
            if (_subscriptions.TryGetValue(key, out ImmutableSortedSet<Func<Task>>? topicSubscriptions))
            {
                var upadateTasks = topicSubscriptions
                    .Select(updateState => updateState())
                    .ToArray();
                await Task.WhenAll(upadateTasks);
            }
        }

        private class Unsubscriber(Action unsubscribe) : IDisposable
        {
            private readonly Action _unsubscribe = unsubscribe;

            public void Dispose() => _unsubscribe();
        }
    }
}
