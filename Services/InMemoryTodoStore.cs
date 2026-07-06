using congenial_octo_engine.Models;

namespace congenial_octo_engine.Services;

public class InMemoryTodoStore : ITodoStore
{
    private readonly List<TodoItem> _items =
    [
        new TodoItem { Title = "Sketch the first-pass MVC Todo experience" },
        new TodoItem { Title = "Swap in new data providers later without changing the UI" },
        new TodoItem { Title = "Keep the color palette cool, clean, and definitely not purple", IsComplete = true }
    ];

    private readonly Lock _lock = new();

    public IReadOnlyList<TodoItem> GetAll()
    {
        lock (_lock)
        {
            return _items
                .OrderBy(item => item.IsComplete)
                .ThenByDescending(item => item.CreatedAt)
                .ToList();
        }
    }

    public void Add(string title)
    {
        var normalizedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return;
        }

        lock (_lock)
        {
            _items.Add(new TodoItem { Title = normalizedTitle });
        }
    }

    public void Toggle(Guid id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(existing => existing.Id == id);
            if (item is null)
            {
                return;
            }

            item.IsComplete = !item.IsComplete;
        }
    }

    public void Delete(Guid id)
    {
        lock (_lock)
        {
            _items.RemoveAll(item => item.Id == id);
        }
    }
}
