using Microsoft.Extensions.Options;

namespace Todo.Worker;

public sealed class TodoPulseWorker(
    ILogger<TodoPulseWorker> logger,
    IOptions<WorkerOptions> options) : BackgroundService
{
    private readonly WorkerOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var run = 0;
        logger.LogInformation(
            "Todo background service started. Scenario: {Scenario}. Interval: {IntervalSeconds}s.",
            _options.Scenario,
            _options.IntervalSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            run++;
            logger.LogInformation(
                "Processed demo todo digest #{Run}. Open={OpenCount}, Completed={CompletedCount}, Service={ServiceName}.",
                run,
                3 + run % 4,
                run % 3,
                _options.ServiceName);
        }
    }
}
