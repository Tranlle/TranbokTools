namespace TOrbit.Core.Services;

public interface IAppShellService
{
    string AppName { get; }
    string WorkspaceRoot { get; }
}
