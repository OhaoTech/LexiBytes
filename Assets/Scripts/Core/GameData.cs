using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public string id;
    public string title;
    public string description;
    public string modelName;
    public string initialPrompt;
    public DateTime createdAt;
    public DateTime lastPlayedAt;
    public List<DialogueEntry> dialogue;

    [Serializable]
    public class DialogueEntry
    {
        public string speaker;
        public string message;
        public DateTime timestamp;
    }

    // Constructor for new games
    public GameData(string title = "New Story", string description = "", string modelName = "mistral")
    {
        this.id = Guid.NewGuid().ToString();
        this.title = title;
        this.description = description;
        this.modelName = modelName;
        this.initialPrompt = "";
        this.createdAt = DateTime.Now;
        this.lastPlayedAt = DateTime.Now;
        this.dialogue = new List<DialogueEntry>();
    }

    // Update last played time
    public void UpdateLastPlayed()
    {
        this.lastPlayedAt = DateTime.Now;
    }
}
