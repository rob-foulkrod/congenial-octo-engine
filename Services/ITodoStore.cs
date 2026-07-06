using congenial_octo_engine.Models;

namespace congenial_octo_engine.Services;

public interface ITodoStore
{
    IReadOnlyList<TodoItem> GetAll();

    void Add(string title);

    void Toggle(Guid id);

    void Delete(Guid id);
}
