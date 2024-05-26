namespace Ty.Services.Configs;


/// <summary>
/// 配置文件的路径
/// </summary>
[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ConfigPathAttribute : Attribute
{
    // See the attribute guidelines at 
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    readonly string path;

    // This is a positional argument
    public ConfigPathAttribute(string path)
    {
        this.path = path;
    }

    public string Path
    {
        get { return path; }
    }
}