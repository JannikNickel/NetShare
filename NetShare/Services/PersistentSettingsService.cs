using NetShare.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace NetShare.Services
{
    public class PersistentSettingsService : ISettingsService
    {
        public Settings? CurrentSettings { get; private set; }

        private static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string SaveDir => Path.Combine(AppData, nameof(NetShare));
        private static string SaveFile => Path.Combine(SaveDir, "settings.json");
        private static JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        private static Encoding Encoding { get; set; } = Encoding.UTF8;

        public void LoadSettings()
        {
            CreateSaveDir();
            CurrentSettings = LoadSettingsOrDefault();
        }

        public void SetSettings(Settings? settings, bool save)
        {
            CurrentSettings = settings;
            if(save && settings != null)
            {
                SaveSettings(settings);
            }
        }

        private static void CreateSaveDir()
        {
            if(!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }
        }

        private static Settings LoadSettingsOrDefault()
        {
            try
            {
                if(File.Exists(SaveFile))
                {
                    string json = File.ReadAllText(SaveFile, Encoding);
                    Settings? settings = JsonSerializer.Deserialize<Settings>(json, SerializerOptions);
                    if(settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch { }
            return Settings.Default;
        }

        private static void SaveSettings(Settings settings)
        {
            CreateSaveDir();
            try
            {
                string json = JsonSerializer.Serialize<Settings>(settings, SerializerOptions);
                File.WriteAllText(SaveFile, json, Encoding);
            }
            catch { }
        }
    }
}
