namespace ToDoList
{
    public static class Util
    {
        public static string FullMessage(this Exception? exception)
        {
            var messages = new List<string>();
            var currentException = exception;
            while (currentException != null)
            {
                messages.Add(currentException.Message);
                currentException = currentException.InnerException;
            }
            return string.Join(" → ", messages);
        }
    }
}
