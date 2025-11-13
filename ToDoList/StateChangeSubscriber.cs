using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using ToDoList.Services;

namespace ToDoList
{
    public abstract class StateChangeSubscriber<TTopic> : ComponentBase, IDisposable where TTopic : notnull
    {
        [Inject]
        // `required` does not actually guarantee anything here
        public required StateChangeNotifier<TTopic> StateChangeNotifier { get; set; }

        private ImmutableSortedSet<IDisposable> _disposables = [];

        public async Task StateChangeSubscribeAsync(TTopic topic, Func<Task> updateState) => await StateChangeSubscribeAsync([topic], updateState);
        public async Task StateChangeSubscribeAsync(IImmutableSet<TTopic> topics, Func<Task> updateState)
        {
            var disposable = await StateChangeNotifier.SubscribeAsync(topics, async () => {
                await updateState();
                await InvokeAsync(() => {
                    StateHasChanged();
                });
            });
            ImmutableInterlocked.Update(ref _disposables, disposables => disposables.Add(disposable));
        }

        public async Task NotifyAsync(TTopic topic) => await NotifyAsync([topic]);
        public async Task NotifyAsync(IImmutableSet<TTopic> topics)
        {
            await StateChangeNotifier.NotifyAsync(topics);
        }

        public void Dispose()
        {
            foreach (var dispoasble in _disposables)
            {
                dispoasble.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
