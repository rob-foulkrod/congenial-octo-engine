using Grpc.Core;
using Todo.Worker.Contracts;
using WorkerGrpc = Todo.Worker.Contracts.TodoWorker;

namespace Todo.Worker;

public sealed class TodoWorkerGrpcService(ILogger<TodoWorkerGrpcService> logger) : WorkerGrpc.TodoWorkerBase
{
    public override Task<TodoCreatedReply> RecordTodoCreated(TodoCreatedRequest request, ServerCallContext context)
    {
        logger.LogInformation(
            "Received TodoCreated over gRPC. Title={Title}, RequestId={RequestId}, Source={Source}.",
            request.Title,
            request.RequestId,
            request.Source);

        return Task.FromResult(new TodoCreatedReply
        {
            Accepted = true,
            Message = "Todo creation recorded by worker."
        });
    }
}
