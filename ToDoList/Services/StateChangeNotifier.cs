namespace ToDoList.Services
{
    public class StateChangeNotifier
    {
        private readonly Dictionary<Type, List<Func<Task>>> _subscriptions = [];

        public async Task<IDisposable> SubscribeAsync(Type topic, Func<Task> updateState)
        {
            var initTask = updateState.Invoke();

            if (!_subscriptions.ContainsKey(topic))
            {
                _subscriptions[topic] = [];
            }
            _subscriptions[topic].Add(updateState);

            await initTask;

            return new Unsubscriber(() =>
            {
                if (_subscriptions.ContainsKey(topic))
                {
                    _subscriptions[topic].Remove(updateState);
                }
            });
        }
        public async Task NotifyAsync(Type key)
        {
            if (_subscriptions.ContainsKey(key))
            {
                var upadateTasks = _subscriptions[key]
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
