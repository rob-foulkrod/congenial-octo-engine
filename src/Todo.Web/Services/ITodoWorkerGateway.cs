namespace Todo.Web.Services;

public interface ITodoWorkerGateway
{
    Task RecordTodoCreatedAsync(string title, string requestId, CancellationToken cancellationToken);
}
