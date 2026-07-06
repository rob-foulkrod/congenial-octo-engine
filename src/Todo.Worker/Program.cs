using Microsoft.AspNetCore.Server.Kestrel.Core;
using Todo.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));
builder.Services.AddHostedService<TodoPulseWorker>();
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<TodoWorkerGrpcService>();
app.MapGet("/", () => "Todo worker gRPC endpoint.");

await app.RunAsync();
