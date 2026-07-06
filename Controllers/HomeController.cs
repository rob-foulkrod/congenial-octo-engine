using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using congenial_octo_engine.Models;
using congenial_octo_engine.Services;

namespace congenial_octo_engine.Controllers;

public class HomeController : Controller
{
    private readonly ITodoStore _todoStore;

    public HomeController(ITodoStore todoStore)
    {
        _todoStore = todoStore;
    }

    public IActionResult Index()
    {
        return View(BuildViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TodoPageViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewItemTitle))
        {
            ModelState.AddModelError(nameof(TodoPageViewModel.NewItemTitle), "Give your task a name.");
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
