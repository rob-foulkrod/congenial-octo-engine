namespace congenial_octo_engine.Models;

public class TodoPageViewModel
{
    public IReadOnlyList<TodoItem> Items { get; init; } = [];

    public string NewItemTitle { get; init; } = string.Empty;

    public int CompletedCount => Items.Count(item => item.IsComplete);

    public int RemainingCount => Items.Count(item => !item.IsComplete);
}
