using TOrbit.Core.Models;

namespace TOrbit.Core.Services;

public interface IAppPreferencesService
{
    AppPreferences Load();
    void Save(AppPreferences preferences);
}
