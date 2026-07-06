using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Todo.Web.Models;
using Todo.Web.Services;

namespace Todo.Web.Controllers;

public class HomeController : Controller
{
    private readonly ITodoStore _todoStore;
    private readonly ITodoWorkerGateway _todoWorkerGateway;

    public HomeController(ITodoStore todoStore, ITodoWorkerGateway todoWorkerGateway)
    {
        _todoStore = todoStore;
        _todoWorkerGateway = todoWorkerGateway;
    }

    public IActionResult Index()
    {
        return View(BuildViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoPageViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewItemTitle))
        {
            ModelState.AddModelError(nameof(TodoPageViewModel.NewItemTitle), "Give your task a name.");
            return View("Index", BuildViewModel(model.NewItemTitle));
        }

        try
        {
            await _todoWorkerGateway.RecordTodoCreatedAsync(model.NewItemTitle.Trim(), HttpContext.TraceIdentifier, HttpContext.RequestAborted);
        }
        catch (RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.Unavailable or Grpc.Core.StatusCode.DeadlineExceeded)
        {
            ModelState.AddModelError(nameof(TodoPageViewModel.NewItemTitle), "The private worker dependency is unavailable. Try again after the worker starts.");
            return View("Index", BuildViewModel(model.NewItemTitle));
        }

        _todoStore.Add(model.NewItemTitle);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(Guid id)
    {
        _todoStore.Toggle(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        _todoStore.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private TodoPageViewModel BuildViewModel(string? newItemTitle = null)
    {
        return new TodoPageViewModel
        {
            Items = _todoStore.GetAll(),
            NewItemTitle = newItemTitle ?? string.Empty
        };
    }
}
