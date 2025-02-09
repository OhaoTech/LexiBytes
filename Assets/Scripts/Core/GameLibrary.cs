using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class GameLibrary : MonoBehaviour
{
    private static GameLibrary instance;
    public static GameLibrary Instance => instance;

    private const string SAVE_DIR = "GameSaves";
    private List<GameData> games = new List<GameData>();

    public event System.Action<GameData> OnGameCreated;
    public event System.Action<GameData> OnGameUpdated;
    public event System.Action<string> OnGameDeleted;

    // Create
    public GameData CreateGame(string title = "New Story", string description = "", string modelName = "mistral")
    {
        var newGame = new GameData(title, description, modelName);
        games.Add(newGame);
        SaveGame(newGame);
        OnGameCreated?.Invoke(newGame);
        return newGame;
    }

    // Read
    public GameData GetGame(string id)
    {
        return games.Find(g => g.id == id);
    }

    public List<GameData> GetAllGames()
    {
        return new List<GameData>(games);
    }

    // Update
    public void UpdateGame(GameData game)
    {
        int index = games.FindIndex(g => g.id == game.id);
        if (index >= 0)
        {
            games[index] = game;
            SaveGame(game);
            OnGameUpdated?.Invoke(game);
        }
    }

    // Delete
    public void DeleteGame(string id)
    {
        int index = games.FindIndex(g => g.id == id);
        if (index >= 0)
        {
            string path = GetSavePath(id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            games.RemoveAt(index);
            OnGameDeleted?.Invoke(id);
        }
    }

    // Save/Load methods
    private string GetSavePath(string id)
    {
        return Path.Combine(Application.persistentDataPath, SAVE_DIR, $"{id}.json");
    }

    public void SaveGame(GameData game)
    {
        string path = GetSavePath(game.id);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json = JsonUtility.ToJson(game, true);
        File.WriteAllText(path, json);
    }

    private void LoadAllGames()
    {
        games.Clear();
        string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
        if (!Directory.Exists(dir)) return;

        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                GameData game = JsonUtility.FromJson<GameData>(json);
                games.Add(game);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading game from {file}: {e.Message}");
            }
        }

        // Sort by last played
        games.Sort((a, b) => b.lastPlayedAt.CompareTo(a.lastPlayedAt));
    }

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
}
