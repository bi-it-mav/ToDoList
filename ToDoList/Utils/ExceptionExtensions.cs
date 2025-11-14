using System.Collections.Immutable;

namespace ToDoList.Utils
{
    public static class ExceptionExtensions
    {
        public static string FullMessage(this Exception? exception)
        {
            (ImmutableList<string> accumulator, Exception? pointer) state = (
                accumulator: [],
                pointer: exception
            );
            while (state.pointer is not null)
            {
                state = (
                    accumulator: state.accumulator.Add(state.pointer.Message),
                    pointer: state.pointer.InnerException
                );
            }
            return string.Join(" → ", state.accumulator);
        }
    }
}
