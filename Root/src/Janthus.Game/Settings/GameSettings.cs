using System.Text.Json;

namespace Janthus.Game.Settings;

public class GameSettings
{
    public int ResolutionIndex { get; set; } = 1; // 1280x720
    public bool IsFullScreen { get; set; }

    private static readonly string DirectoryPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Janthus");

    private static readonly string FilePath =
        Path.Combine(DirectoryPath, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static GameSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions);
                if (settings != null)
                    return settings;
            }
        }
        catch
        {
            // Corrupt or unreadable — return defaults
        }

        return new GameSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Non-critical — silently ignore write failures
        }
    }
}
