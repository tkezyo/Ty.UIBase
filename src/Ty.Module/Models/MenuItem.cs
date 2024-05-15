namespace Ty.Module.Models;

public class MenuOptions
{
    public List<MenuItem> MenuItems { get; set; } = [];
}
public class MenuItem(string name, string? displayName = null, string? url = null, string? icon = null)
{
    public string Name { get; set; } = name;

    public string DisplayName { get; set; } = displayName ?? name;

    public string? Url { get; set; } = url;

    public string? Icon { get; set; } = icon;

    public List<string> RequiredPermissionNames { get; set; } = [];

    public IReadOnlyList<MenuItem> Items => Items;

    private readonly List<MenuItem> _items = [];

    public MenuItem AddItem(MenuItem item)
    {
        //Check if there is already a menu item with the same name
        if (Items.Any(c => c.Name == item.Name))
        {
            throw new Exception($"There is already a menu item with the name: {item.Name}");
        }

        _items.Add(item);
        return this;
    }

    public void RemoveItem(MenuItem item)
    {
        _items.Remove(item);
    }

    public MenuItem? Find(string name)
    {
        return Items.FirstOrDefault(c => c.Name == name);
    }
}
