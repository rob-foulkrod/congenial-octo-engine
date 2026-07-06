using Todo.Worker.Contracts;

namespace Todo.Web.Services;

public sealed class GrpcTodoWorkerGateway(TodoWorker.TodoWorkerClient client) : ITodoWorkerGateway
{
    public async Task RecordTodoCreatedAsync(string title, string requestId, CancellationToken cancellationToken)
    {
        var reply = await client.RecordTodoCreatedAsync(
            new TodoCreatedRequest
            {
                Title = title,
                RequestId = requestId,
                Source = "todo-web"
            },
            cancellationToken: cancellationToken);

        if (!reply.Accepted)
        {
            throw new InvalidOperationException($"Worker rejected todo creation: {reply.Message}");
        }
    }
}
