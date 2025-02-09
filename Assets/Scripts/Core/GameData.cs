using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public string id;                     // Unique identifier
    public string title;                  // Game title
    public string description;            // Short description
    public string modelName;              // LLM model used
    public string initialPrompt;          // Starting prompt
    public DateTime createdAt;            // Creation date
    public DateTime lastPlayedAt;         // Last played date
    public List<DialogueEntry> dialogue;  // Conversation history

    [Serializable]
    public class DialogueEntry
    {
        public string speaker;    // "You" or "Narrator"
        public string message;    // The actual text
        public DateTime timestamp;
    }
}
