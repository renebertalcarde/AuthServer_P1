using System.Reflection;

namespace HRMobile.Services;

public class AssemblyService
{
    public string AppTitle => $"{AppName} v{AppVersion}";
    public string AppVersion => Version.ToString() ?? "[AppVersion]";
    public string AppName =>
        _assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ??
        _assembly?.FullName ??
        "[AppName]";
    public Version Version => _assembly.GetName().Version ?? new Version(0, 0, 0, 0);

    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

}