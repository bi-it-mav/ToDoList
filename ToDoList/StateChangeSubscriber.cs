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

        public async Task StateChangeSubscribeAsync(TTopic topic, Func<Task> updateState)
        {
            var disposable = await StateChangeNotifier.SubscribeAsync(topic, async () => {
                await updateState();
                await InvokeAsync(() => {
                    StateHasChanged();
                });
            });
            ImmutableInterlocked.Update(ref _disposables, disposables => disposables.Add(disposable));
        }

        public async Task NotifyAsync(TTopic topic)
        {
            await StateChangeNotifier.NotifyAsync(topic);
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
