using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class GameplayManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private ScrollRect scrollRect;
    private LLMService llmService;

    [Header("Story Settings")]
    [SerializeField] private string initialPrompt = "You are a traveler who just arrived in a mysterious town. What would you like to do?";

    private string conversationHistory = "";
    private bool isInitialized = false;
    private bool isGenerating = false;


    private void Awake()
    {
        llmService = gameObject.AddComponent<LLMService>();
    }

    private void InitializeUI()
    {
        if (submitButton != null && playerInput != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            playerInput.onSubmit.AddListener(OnInputSubmit);
        }
    }

    private void StartStory()
    {
        if (storyText != null)
        {
            storyText.text = "";
            AppendToStory(initialPrompt);
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

        string prompt = $"You are a creative storyteller narrating an interactive story. " +
                       $"Previous context: {conversationHistory}\n" +
                       $"Player action: {userInput}\n" +
                       $"Respond with a creative and engaging continuation of the story in 2-3 sentences.";

        AppendToStory("\nNarrator: ");

        // Create a StringBuilder to accumulate the full response
        StringBuilder fullResponse = new StringBuilder();

        await llmService.StreamResponse(
            prompt,
            (token) =>
            {
                // Update the UI with each new token
                fullResponse.Append(token);
                UpdateLastLine(fullResponse.ToString());
            },
            () =>
            {
                // Complete the response
                isGenerating = false;
                submitButton.interactable = true;
                playerInput.interactable = true;

                // Add the full response to conversation history
                conversationHistory += "\nNarrator: " + fullResponse.ToString();
                FocusInputField();

            }
        );
    }

    public void SetupReferences(TextMeshProUGUI storyText, TMP_InputField playerInput, Button submitButton, ScrollRect scrollRect)
    {
        this.storyText = storyText;
        this.playerInput = playerInput;
        this.submitButton = submitButton;
        this.scrollRect = scrollRect;

        if (!isInitialized && storyText != null && playerInput != null && submitButton != null && scrollRect != null)
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
}
