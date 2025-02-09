using UnityEngine;
using System;

public class NewStorySetupWindow : MonoBehaviour
{
    private bool isVisible = false;
    private Rect windowRect;
    private const float WIDTH = 400;
    private const float HEIGHT = 300;

    private string storyTitle = "New Story";
    private string storyDescription = "";
    private string initialPrompt = "You are a traveler who just arrived in a mysterious town. What would you like to do?";
    private string selectedModel = "mistral";

    private GUIStyle windowStyle;
    private GUIStyle labelStyle;
    private GUIStyle inputStyle;
    private GUIStyle buttonStyle;

    private Action<GameData> onComplete;
    private Action onCancel;

    private void Awake()
    {
        // Center the window
        windowRect = new Rect(
            (Screen.width - WIDTH) / 2,
            (Screen.height - HEIGHT) / 2,
            WIDTH,
            HEIGHT
        );

        InitializeStyles();
    }

    private void InitializeStyles()
    {
        windowStyle = new GUIStyle(GUI.skin.window)
        {
            padding = new RectOffset(20, 20, 20, 20)
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 10, 5)
        };
        labelStyle.normal.textColor = Color.white;

        inputStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 0, 10)
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            padding = new RectOffset(15, 15, 8, 8)
        };
    }

    public void Show(Action<GameData> onComplete, Action onCancel = null)
    {
        this.onComplete = onComplete;
        this.onCancel = onCancel;
        isVisible = true;
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        // Darken background
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw window
        windowRect = GUI.Window(0, windowRect, DrawWindowContent, "New Story Setup", windowStyle);
    }

    private void DrawWindowContent(int windowID)
    {
        float y = 30;
        float inputHeight = 24;

        // Title
        GUI.Label(new Rect(10, y, WIDTH - 20, 20), "Story Title:", labelStyle);
        y += 25;
        storyTitle = GUI.TextField(new Rect(10, y, WIDTH - 40, inputHeight), storyTitle, inputStyle);
        y += 35;

        // Description
        GUI.Label(new Rect(10, y, WIDTH - 20, 20), "Description:", labelStyle);
        y += 25;
        storyDescription = GUI.TextArea(new Rect(10, y, WIDTH - 40, inputHeight * 2), storyDescription, inputStyle);
        y += 65;

        // Initial Prompt
        GUI.Label(new Rect(10, y, WIDTH - 20, 20), "Initial Prompt:", labelStyle);
        y += 25;
        initialPrompt = GUI.TextArea(new Rect(10, y, WIDTH - 40, inputHeight * 2), initialPrompt, inputStyle);
        y += 65;

        // Buttons
        float buttonWidth = (WIDTH - 60) / 2;
        if (GUI.Button(new Rect(10, HEIGHT - 50, buttonWidth, 30), "Cancel", buttonStyle))
        {
            Close();
        }

        if (GUI.Button(new Rect(WIDTH - buttonWidth - 30, HEIGHT - 50, buttonWidth, 30), "Create", buttonStyle))
        {
            CreateNewStory();
        }

        // Make window draggable
        GUI.DragWindow();
    }

    private void CreateNewStory()
    {
        var newGame = GameLibrary.Instance.CreateGame(
            storyTitle,
            storyDescription,
            selectedModel
        );

        // Set initial prompt
        newGame.initialPrompt = initialPrompt;

        // Save the game
        GameLibrary.Instance.UpdateGame(newGame);

        // Call completion callback
        onComplete?.Invoke(newGame);
        Close();
    }

    private void Close()
    {
        isVisible = false;
        onCancel?.Invoke();
    }
}
