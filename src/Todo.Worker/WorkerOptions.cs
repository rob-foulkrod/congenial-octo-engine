namespace Todo.Worker;

public sealed class WorkerOptions
{
    public string ServiceName { get; init; } = "todo-worker";

    public string Scenario { get; init; } = "Emit periodic todo digest logs for an Azure Container Apps classroom demo.";

    public int IntervalSeconds { get; init; } = 15;
}
