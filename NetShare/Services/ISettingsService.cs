using NetShare.Models;

namespace NetShare.Services
{
    public interface ISettingsService
    {
        Settings? CurrentSettings { get; }

        void LoadSettings();
        void SetSettings(Settings? settings, bool save);
    }
}
