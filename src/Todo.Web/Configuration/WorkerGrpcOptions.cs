namespace Todo.Web.Configuration;

public sealed class WorkerGrpcOptions
{
    public string Address { get; init; } = "http://octo-engine-worker";
}
