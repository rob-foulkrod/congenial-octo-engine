using Todo.Web.Models;

namespace Todo.Web.Services;

public interface ITodoStore
{
    IReadOnlyList<TodoItem> GetAll();

    void Add(string title);

    void Toggle(Guid id);

    void Delete(Guid id);
}
