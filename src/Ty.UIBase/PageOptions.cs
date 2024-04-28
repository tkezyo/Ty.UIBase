namespace Ty;

public class PageOptions
{
    /// <summary>
    /// 登录页面
    /// </summary>
    public Type? FirstLoadPage { get; set; }
    public object? Loading { get; set; }
    /// <summary>
    /// 布局页面
    /// </summary>
    public Type? LayoutPage { get; set; }
    public string? Title { get; set; }
    public int? Hight { get; set; }
    public int? Width { get; set; }
}
