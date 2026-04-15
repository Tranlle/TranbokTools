using TOrbit.Core.Constants;

namespace TOrbit.Core.Services;

public sealed class AppShellService : IAppShellService
{
    public string AppName => ToolHostConstants.HostName;
    public string WorkspaceRoot => AppContext.BaseDirectory;
}
