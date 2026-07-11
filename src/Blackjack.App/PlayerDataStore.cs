using System.IO;
using System.Text.Json;

namespace Blackjack.App;

public sealed class PlayerDataStore
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    private readonly string _filePath;

    public PlayerDataStore()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "BlackjackRemastered");

        _filePath = Path.Combine(
            directory,
            "player-data.json");
    }

    public string FilePath => _filePath;

    public PlayerData Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return CreateDefaultData();
            }

            string json = File.ReadAllText(
                _filePath);

            PlayerData? data =
                JsonSerializer.Deserialize<PlayerData>(
                    json,
                    JsonOptions);

            if (data is null)
            {
                return CreateDefaultData();
            }

            data.Normalize();
            return data;
        }
        catch (
            IOException)
        {
            return CreateDefaultData();
        }
        catch (
            UnauthorizedAccessException)
        {
            return CreateDefaultData();
        }
        catch (
            JsonException)
        {
            return CreateDefaultData();
        }
    }

    public bool Save(PlayerData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            string? directory =
                Path.GetDirectoryName(
                    _filePath);

            if (!string.IsNullOrWhiteSpace(
                directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            string json =
                JsonSerializer.Serialize(
                    data,
                    JsonOptions);

            string temporaryPath =
                _filePath + ".tmp";

            File.WriteAllText(
                temporaryPath,
                json);

            File.Move(
                temporaryPath,
                _filePath,
                overwrite: true);

            return true;
        }
        catch (
            IOException)
        {
            return false;
        }
        catch (
            UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static PlayerData CreateDefaultData()
    {
        PlayerData data = new();
        data.Normalize();
        data.SessionStatistics.Reset(
            data.CurrentBankroll);

        data.LifetimeStatistics.Reset(
            data.CurrentBankroll);

        return data;
    }
}
