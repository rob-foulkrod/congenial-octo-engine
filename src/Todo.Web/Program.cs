using Microsoft.Extensions.Options;
using Todo.Web.Configuration;
using Todo.Web.Services;
using Todo.Worker.Contracts;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITodoStore, InMemoryTodoStore>();
builder.Services.Configure<WorkerGrpcOptions>(builder.Configuration.GetSection("WorkerGrpc"));
builder.Services.AddGrpcClient<TodoWorker.TodoWorkerClient>((services, options) =>
{
    var workerOptions = services.GetRequiredService<IOptions<WorkerGrpcOptions>>().Value;
    options.Address = new Uri(workerOptions.Address);
});
builder.Services.AddScoped<ITodoWorkerGateway, GrpcTodoWorkerGateway>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "todo-web" }));
app.MapGet("/workerz", async (ITodoWorkerGateway workerGateway, HttpContext httpContext) =>
{
    await workerGateway.RecordTodoCreatedAsync("worker dependency check", httpContext.TraceIdentifier, httpContext.RequestAborted);
    return Results.Ok(new { status = "ok", dependency = "todo-worker-grpc" });
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
