using System.Collections.Immutable;
using ToDoList.Utils;

namespace ToDoList.Services
{
    public class StateChangeNotifier<TTopic> where TTopic : notnull
    {
        private static readonly ImmutableSortedSet<Func<Task>> EmptyTopicSubscriptions = ImmutableSortedSet<Func<Task>>.Empty
            .WithComparer(ValueThenIdentityComparer<Func<Task>>.Instance);
        
        private readonly Atomic<ImmutableDictionary<TTopic, ImmutableSortedSet<Func<Task>>>> _subscriptions = new([]);

        public async Task<IDisposable> SubscribeAsync(TTopic topic, Func<Task> updateState) => await SubscribeAsync([topic], updateState);

        public async Task<IDisposable> SubscribeAsync(IImmutableSet<TTopic> topics, Func<Task> updateState)
        {
            var initTask = updateState.Invoke();

            foreach (var topic in topics)
            {
                _subscriptions.Update(dict => {
                    if (dict.TryGetValue(topic, out var topicSubscriptions))
                    {
                        return dict.Add(topic, topicSubscriptions.Add(updateState));
                    }    
                    return dict.Add(topic, EmptyTopicSubscriptions.Add(updateState)); 
                });
            }

            await initTask;

            return new Unsubscriber(() =>
            {
                foreach (var topic in topics)
                {
                    _subscriptions.Update(dict =>
                    {
                        if (dict.TryGetValue(topic, out var topicSubscriptions))
                        {
                            var updatedTopicSubscriptions = topicSubscriptions.Remove(updateState);
                            if (updatedTopicSubscriptions.IsEmpty)
                            {
                                return dict.Remove(topic);
                            }
                            return dict.Add(topic, updatedTopicSubscriptions);
                        }
                        return dict;
                    });
                }
            });
        }

        public async Task NotifyAsync(TTopic topic) => await NotifyAsync([topic]);

        public async Task NotifyAsync(IImmutableSet<TTopic> topics)
        {
            var upadateFunctions = topics
                .ToImmutableSortedSet(ValueThenIdentityComparer<TTopic>.Instance)
                .Select(topic => _subscriptions.Value.GetValueOrDefault(topic, EmptyTopicSubscriptions))
                .Aggregate(EmptyTopicSubscriptions, (acc, fs) => acc.Union(fs));
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
    }
}
