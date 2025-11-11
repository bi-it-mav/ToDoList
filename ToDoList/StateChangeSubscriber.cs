using Microsoft.AspNetCore.Components;
using ToDoList.Services;

namespace ToDoList
{
    public abstract class StateChangeSubscriber : ComponentBase, IDisposable
    {
        [Inject]
        // `required` does not actually guarantee anything here
        public required StateChangeNotifier StateChangeNotifier { get; set; }

        private readonly List<IDisposable> _disposables = [];

        public async Task StateChangeSubscribeAsync(Type topic, Func<Task> updateState)
        {
            _disposables.Add(await StateChangeNotifier.SubscribeAsync(topic, async () => {
                await updateState();
                StateHasChanged();
            }));
        }

        public void Dispose()
        {
            _disposables.ForEach(d => d.Dispose());
            GC.SuppressFinalize(this);
        }
    }
}
