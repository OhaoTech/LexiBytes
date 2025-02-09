using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;
using System.Linq;

public class GameplayManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button setupButton;
    private LLMService llmService;

    private string conversationHistory = "";
    private GameData currentGame;
    private bool isInitialized = false;
    private bool isGenerating = false;


    private void Awake()
    {
        llmService = gameObject.AddComponent<LLMService>();
        LoadOrCreateGame();
    }

    private void LoadOrCreateGame()
    {
        string gameId = PlayerPrefs.GetString("SelectedGameId", "");
        if (!string.IsNullOrEmpty(gameId))
        {
            currentGame = GameLibrary.Instance.GetGame(gameId);
        }

        if (currentGame == null)
        {
            // Create new game if none exists
            currentGame = GameLibrary.Instance.CreateGame();
        }

        // Update last played time
        currentGame.UpdateLastPlayed();
        GameLibrary.Instance.SaveGame(currentGame);
    }

    private void InitializeUI()
    {
        if (submitButton != null && playerInput != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            playerInput.onSubmit.AddListener(OnInputSubmit);

            // Add setup button handler
            if (setupButton != null)
            {
                setupButton.onClick.AddListener(ShowSetupWindow);
            }
        }
    }

    private void ShowSetupWindow()
    {
        var setupWindow = FindObjectOfType<StorySetupWindow>();
        if (setupWindow == null)
        {
            var windowObj = new GameObject("StorySetup");
            setupWindow = windowObj.AddComponent<StorySetupWindow>();
        }

        setupWindow.LoadGameData(currentGame);
        setupWindow.Show(
            (updatedGame) =>
            {
                currentGame = updatedGame;
                RefreshStoryDisplay();
            },
            () =>
            {
                Destroy(setupWindow.gameObject);
            }
        );
    }

    private void RefreshStoryDisplay()
    {
        // Clear and reload the story text
        if (storyText != null)
        {
            storyText.text = "";
            LoadConversationHistory();
        }
    }

    private void StartStory()
    {
        if (storyText != null)
        {
            storyText.text = "";

            // If this is a new game, use initial prompt
            if (currentGame.dialogue.Count == 0)
            {
                string initialPrompt = string.IsNullOrEmpty(currentGame.initialPrompt)
                    ? "You are a traveler who just arrived in a mysterious town. What would you like to do?"
                    : currentGame.initialPrompt;

                AppendToStory(initialPrompt);

                // Save initial prompt
                currentGame.dialogue.Add(new GameData.DialogueEntry
                {
                    speaker = "Narrator",
                    message = initialPrompt,
                    timestamp = DateTime.Now
                });
                GameLibrary.Instance.SaveGame(currentGame);
            }
            else
            {
                // Load existing conversation
                LoadConversationHistory();
            }
        }
    }

    private void LoadConversationHistory()
    {
        foreach (var entry in currentGame.dialogue)
        {
            AppendToStory($"\n{entry.speaker}: {entry.message}");
        }
    }

    private void OnSubmitClicked()
    {
        ProcessPlayerInput();
    }

    private void OnInputSubmit(string input)
    {
        ProcessPlayerInput();
    }

    private void ProcessPlayerInput()
    {
        if (string.IsNullOrEmpty(playerInput.text)) return;

        string playerResponse = playerInput.text;
        AppendToStory($"\nYou: {playerResponse}");

        // Save player's message
        currentGame.dialogue.Add(new GameData.DialogueEntry
        {
            speaker = "You",
            message = playerResponse,
            timestamp = DateTime.Now
        });
        GameLibrary.Instance.SaveGame(currentGame);

        // Clear input field
        playerInput.text = "";

        // Generate AI response
        GenerateResponse(playerResponse);

        // Focus input field again
        FocusInputField();
    }

    private void FocusInputField()
    {
        // Select the input field and move cursor to end
        playerInput.Select();
        playerInput.ActivateInputField();
        // Optional: Move cursor to end of text
        playerInput.caretPosition = playerInput.text.Length;
    }

    private void AppendToStory(string text)
    {
        if (storyText == null) return;

        if (string.IsNullOrEmpty(storyText.text))
        {
            storyText.text = text;
        }
        else
        {
            storyText.text += "\n" + text;
        }

        conversationHistory += "\n" + text;

        // Scroll to bottom
        if (scrollRect != null)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    private IEnumerator ScrollToBottom()
    {
        if (scrollRect == null) yield break;

        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    private async void GenerateResponse(string userInput)
    {
        if (isGenerating) return;

        submitButton.interactable = false;
        playerInput.interactable = false;
        isGenerating = true;

        // Build context from dialogue history
        StringBuilder contextBuilder = new StringBuilder();
        contextBuilder.AppendLine($"Title: {currentGame.title}");
        if (!string.IsNullOrEmpty(currentGame.description))
        {
            contextBuilder.AppendLine($"Description: {currentGame.description}");
        }

        foreach (var entry in currentGame.dialogue.TakeLast(5))
        {
            contextBuilder.AppendLine($"{entry.speaker}: {entry.message}");
        }

        string prompt = $"You are a creative storyteller narrating an interactive story. " +
                       $"Previous context: {contextBuilder}\n" +
                       $"Player action: {userInput}\n" +
                       $"Respond with a creative and engaging continuation of the story in 2-3 sentences.";

        AppendToStory("\nNarrator: ");

        StringBuilder fullResponse = new StringBuilder();

        await llmService.StreamResponse(
            prompt,
            (token) =>
            {
                fullResponse.Append(token);
                UpdateLastLine(fullResponse.ToString());
            },
            () =>
            {
                // Save narrator's response
                currentGame.dialogue.Add(new GameData.DialogueEntry
                {
                    speaker = "Narrator",
                    message = fullResponse.ToString(),
                    timestamp = DateTime.Now
                });
                GameLibrary.Instance.SaveGame(currentGame);

                isGenerating = false;
                submitButton.interactable = true;
                playerInput.interactable = true;
                FocusInputField();
            }
        );
    }

    public void SetupReferences(TextMeshProUGUI storyText, TMP_InputField playerInput,
        Button submitButton, ScrollRect scrollRect, Button setupButton)
    {
        this.storyText = storyText;
        this.playerInput = playerInput;
        this.submitButton = submitButton;
        this.scrollRect = scrollRect;
        this.setupButton = setupButton;

        if (!isInitialized && storyText != null && playerInput != null &&
            submitButton != null && scrollRect != null && setupButton != null)
        {
            InitializeUI();
            StartStory();
            isInitialized = true;
            FocusInputField();
        }
        else
        {
            Debug.LogError("Some UI components are missing during setup!");
        }
    }

    private void UpdateLastLine(string text)
    {
        if (storyText == null) return;

        // Split the text into lines
        string[] lines = storyText.text.Split('\n');

        // Replace the last line with the updated text
        if (lines.Length > 0)
        {
            lines[lines.Length - 1] = "Narrator: " + text;
            storyText.text = string.Join("\n", lines);
        }

        // Scroll to bottom
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(2, 0);
        }
    }

    private void OnDestroy()
    {
        // Save game state when exiting
        if (currentGame != null)
        {
            GameLibrary.Instance.SaveGame(currentGame);
        }
    }
}
