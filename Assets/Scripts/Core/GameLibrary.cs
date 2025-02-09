using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameLibrary : MonoBehaviour
{
    private static GameLibrary instance;
    public static GameLibrary Instance => instance;

    private const string SAVE_DIR = "GameSaves";
    private List<GameData> games = new List<GameData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllGames();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<GameData> GetAllGames()
    {
        return new List<GameData>(games);
    }

    public void SaveGame(GameData game)
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_DIR, $"{game.id}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json = JsonUtility.ToJson(game, true);
        File.WriteAllText(path, json);

        // Update in-memory list
        int index = games.FindIndex(g => g.id == game.id);
        if (index >= 0)
            games[index] = game;
        else
            games.Add(game);
    }

    public GameData LoadGame(string id)
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_DIR, $"{id}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    private void LoadAllGames()
    {
        string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
        if (!Directory.Exists(dir))
            return;

        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            string json = File.ReadAllText(file);
            GameData game = JsonUtility.FromJson<GameData>(json);
            games.Add(game);
        }
    }
}
