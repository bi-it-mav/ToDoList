using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using ToDoList.Services;

namespace ToDoList
{
    public abstract class StateChangeSubscriber : ComponentBase, IDisposable
    {
        [Inject]
        // `required` does not actually guarantee anything here
        public required StateChangeNotifier StateChangeNotifier { get; set; }

        private ImmutableSortedSet<IDisposable> _disposables = [];

        public async Task StateChangeSubscribeAsync(Type topic, Func<Task> updateState)
        {
            var disposable = await StateChangeNotifier.SubscribeAsync(topic, async () => {
                await updateState();
                await InvokeAsync(() => {
                    StateHasChanged();
                });
            });
            ImmutableInterlocked.Update(ref _disposables, disposables => disposables.Add(disposable));
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
