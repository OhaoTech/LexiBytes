using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UILayoutManager : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.06f, 0.06f, 0.06f, 1f); // Darker background
    [SerializeField] private Color panelColor = new Color(0.12f, 0.12f, 0.14f, 0.95f); // Subtle panel
    [SerializeField] private Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f); // Bright blue accent
    [SerializeField] private Color inputBackgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);

    [Header("Dimensions")]
    [SerializeField] private float padding = 16f;
    [SerializeField] private float inputHeight = 50f;
    [SerializeField] private float buttonWidth = 120f;
    [SerializeField] private float cornerRadius = 8f;
    [SerializeField] private float backButtonWidth = 80f;
    [SerializeField] private float backButtonHeight = 40f;


    [Header("Prefabs")]
    [SerializeField] private GameObject modelSelectorPrefab;
    [SerializeField] private GameObject storyPanelPrefab;



    private void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        // Setup Canvas
        Canvas canvas = SetupCanvas();

        // Create background
        GameObject backgroundObj = CreatePanel("Background", canvas.transform);
        Image background = backgroundObj.GetComponent<Image>();
        background.color = backgroundColor;
        SetFullScreen(backgroundObj.GetComponent<RectTransform>());

        // Main container with padding
        GameObject mainPanel = CreatePanel("MainPanel", backgroundObj.transform);
        SetupMainLayout(mainPanel);

        // Create header panel for back button and model selector
        GameObject headerPanel = CreateHeaderPanel(mainPanel.transform);

        // Story Panel
        GameObject storyPanel = Instantiate(storyPanelPrefab, mainPanel.transform);
        var storyPanelUI = storyPanel.GetComponent<StoryPanelUI>();

        // Input Area
        GameObject inputContainer = CreateInputArea(mainPanel.transform);
        TMP_InputField inputField = inputContainer.GetComponentInChildren<TMP_InputField>();
        Button submitButton = inputContainer.GetComponentInChildren<Button>();

        // Setup GameplayManager
        SetupGameplayManager(storyPanelUI.storyText, inputField, submitButton, storyPanelUI.scrollRect);
    }

    private void SetupMainLayout(GameObject mainPanel)
    {
        var mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
        mainLayout.padding = new RectOffset((int)padding * 2, (int)padding * 2, (int)padding * 2, (int)padding * 2);
        mainLayout.spacing = padding;
        mainLayout.childControlHeight = true;
        mainLayout.childForceExpandHeight = false;
        SetFullScreen(mainPanel.GetComponent<RectTransform>());
    }

    private GameObject CreateHeaderPanel(Transform parent)
    {
        GameObject headerPanel = CreatePanel("HeaderPanel", parent);

        // Setup horizontal layout
        HorizontalLayoutGroup headerLayout = headerPanel.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        headerLayout.spacing = padding;
        headerLayout.childControlWidth = false;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;

        // Add layout element to control height
        LayoutElement headerElement = headerPanel.AddComponent<LayoutElement>();
        headerElement.minHeight = backButtonHeight + padding * 2;
        headerElement.flexibleHeight = 0;

        // Create back button inside header
        GameObject backButton = CreateBackButtonNew(headerPanel.transform);

        // Create model selector inside header
        GameObject modelSelector = Instantiate(modelSelectorPrefab, headerPanel.transform);

        return headerPanel;
    }

    private GameObject CreateBackButtonNew(Transform parent)
    {
        GameObject backButtonObj = CreatePanel("BackButton", parent);

        // Setup button
        Button backButton = backButtonObj.AddComponent<Button>();
        Image buttonImage = backButtonObj.GetComponent<Image>();
        buttonImage.color = accentColor;
        buttonImage.sprite = CreateRoundedRectSprite(cornerRadius);
        buttonImage.type = Image.Type.Sliced;

        // Add text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(backButtonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Back";
        buttonText.color = Color.white;
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

        // Position text
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;

        // Layout element for size control
        LayoutElement buttonLayout = backButtonObj.AddComponent<LayoutElement>();
        buttonLayout.minWidth = backButtonWidth;
        buttonLayout.minHeight = backButtonHeight;
        buttonLayout.flexibleWidth = 0;

        // Add click handler
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Add hover animation
        AddButtonAnimation(backButton);

        return backButtonObj;
    }

    private GameObject CreateInputArea(Transform parent)
    {
        GameObject inputPanel = CreatePanel("InputPanel", parent);
        HorizontalLayoutGroup inputLayout = inputPanel.AddComponent<HorizontalLayoutGroup>();
        inputLayout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        inputLayout.spacing = padding;
        inputLayout.childControlWidth = true;
        inputLayout.childForceExpandWidth = false;

        // Add rounded corners
        Image inputPanelImage = inputPanel.GetComponent<Image>();
        inputPanelImage.color = panelColor;
        inputPanelImage.sprite = CreateRoundedRectSprite(cornerRadius);
        inputPanelImage.type = Image.Type.Sliced;

        // Input field
        GameObject inputFieldObj = CreateInputField(inputPanel.transform);

        // Submit button
        GameObject buttonObj = CreateButton("SubmitButton", inputPanel.transform);

        // Set fixed height
        LayoutElement inputPanelLayout = inputPanel.AddComponent<LayoutElement>();
        inputPanelLayout.minHeight = inputHeight + padding * 2;
        inputPanelLayout.flexibleHeight = 0;

        return inputPanel;
    }

    private GameObject CreateInputField(Transform parent)
    {
        GameObject inputFieldObj = CreatePanel("InputField", parent);

        // Background
        Image inputBackground = inputFieldObj.GetComponent<Image>();
        inputBackground.color = inputBackgroundColor;
        inputBackground.sprite = CreateRoundedRectSprite(cornerRadius);
        inputBackground.type = Image.Type.Sliced;

        // Input field component
        TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();

        // Text area
        GameObject textArea = new GameObject("TextArea", typeof(RectTransform));
        textArea.transform.SetParent(inputFieldObj.transform);
        TextMeshProUGUI inputText = textArea.AddComponent<TextMeshProUGUI>();
        inputText.color = textColor;
        inputText.fontSize = 16;
        inputText.margin = new Vector4(padding, padding / 2, padding, padding / 2);
        ResetRectTransform(textArea.GetComponent<RectTransform>());

        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(inputFieldObj.transform);
        TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Type your action here...";
        placeholder.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        placeholder.fontSize = 16;
        placeholder.margin = new Vector4(padding, padding / 2, padding, padding / 2);
        ResetRectTransform(placeholderObj.GetComponent<RectTransform>());

        // Setup input field
        inputField.textViewport = textArea.GetComponent<RectTransform>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;

        // Layout settings
        LayoutElement inputLayout = inputFieldObj.AddComponent<LayoutElement>();
        inputLayout.flexibleWidth = 1;

        return inputFieldObj;
    }

    private GameObject CreateButton(string name, Transform parent)
    {
        GameObject buttonObj = CreatePanel(name, parent);

        // Button component
        Button button = buttonObj.AddComponent<Button>();

        // Button background
        Image buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.color = accentColor;
        buttonImage.sprite = CreateRoundedRectSprite(cornerRadius);
        buttonImage.type = Image.Type.Sliced;

        // Button text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Send";
        buttonText.color = Color.white;
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        ResetRectTransform(textObj.GetComponent<RectTransform>());

        // Layout settings
        LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
        buttonLayout.minWidth = buttonWidth;
        buttonLayout.flexibleWidth = 0;

        // Button animation
        AddButtonAnimation(button);

        return buttonObj;
    }

    private void AddButtonAnimation(Button button)
    {
        // Add hover animation
        ColorBlock colors = button.colors;
        colors.normalColor = accentColor;
        colors.highlightedColor = new Color(
            accentColor.r * 1.2f,
            accentColor.g * 1.2f,
            accentColor.b * 1.2f,
            accentColor.a
        );
        colors.pressedColor = new Color(
            accentColor.r * 0.8f,
            accentColor.g * 0.8f,
            accentColor.b * 0.8f,
            accentColor.a
        );
        colors.selectedColor = accentColor;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }

    private void CreateScrollbar(GameObject storyPanel, ScrollRect scrollRect)
    {
        GameObject scrollbarObj = CreatePanel("Scrollbar", storyPanel.transform);
        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();

        // Scrollbar background
        Image scrollbarImage = scrollbarObj.GetComponent<Image>();
        scrollbarImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        // Scrollbar handle
        GameObject handleObj = CreatePanel("Handle", scrollbarObj.transform);
        Image handleImage = handleObj.GetComponent<Image>();
        handleImage.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);

        // Setup scrollbar
        scrollbar.handleRect = handleObj.GetComponent<RectTransform>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        // Position scrollbar
        RectTransform scrollbarRect = scrollbarObj.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.sizeDelta = new Vector2(10, 0);
        scrollbarRect.anchoredPosition = new Vector2(0, 0);

        // Connect to scroll rect
        scrollRect.verticalScrollbar = scrollbar;
    }

    // Helper method to create rounded rectangle sprite
    private Sprite CreateRoundedRectSprite(float radius)
    {
        // Create a white pixel texture that can be tinted
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[1024];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, new Vector4(radius, radius, radius, radius));
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent);
        Image image = obj.AddComponent<Image>();
        image.color = Color.clear;
        ResetRectTransform(obj.GetComponent<RectTransform>());
        return obj;
    }

    private void SetFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }
    private Canvas SetupCanvas()
    {
        // Get or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        return canvas;
    }

    private void SetupGameplayManager(TextMeshProUGUI storyText, TMP_InputField inputField, Button submitButton, ScrollRect scrollRect)
    {
        GameplayManager gameplayManager = FindObjectOfType<GameplayManager>();
        if (gameplayManager != null)
        {
            StartCoroutine(SetupGameplayManagerDelayed(storyText, inputField, submitButton, scrollRect));
        }
        else
        {
            Debug.LogError("GameplayManager not found in the scene!");
        }
    }

    private IEnumerator SetupGameplayManagerDelayed(TextMeshProUGUI storyText, TMP_InputField inputField, Button submitButton, ScrollRect scrollRect)
    {
        yield return new WaitForEndOfFrame();

        GameplayManager gameplayManager = FindObjectOfType<GameplayManager>();
        gameplayManager.SetupReferences(storyText, inputField, submitButton, scrollRect);
    }

    private void CreateBackButton(Transform parent)
    {
        GameObject backButtonObj = CreatePanel("BackButton", parent);

        // Position in top-left corner
        RectTransform backButtonRT = backButtonObj.GetComponent<RectTransform>();
        backButtonRT.anchorMin = new Vector2(0, 1);
        backButtonRT.anchorMax = new Vector2(0, 1);
        backButtonRT.pivot = new Vector2(0, 1);
        backButtonRT.sizeDelta = new Vector2(backButtonWidth, backButtonHeight);
        backButtonRT.anchoredPosition = new Vector2(padding, -padding);

        // Setup button
        Button backButton = backButtonObj.AddComponent<Button>();
        Image buttonImage = backButtonObj.GetComponent<Image>();
        buttonImage.color = accentColor;
        buttonImage.sprite = CreateRoundedRectSprite(cornerRadius);
        buttonImage.type = Image.Type.Sliced;

        // Add text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(backButtonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Back";
        buttonText.color = Color.white;
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

        // Position text
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;

        // Add click handler
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Add hover animation
        AddButtonAnimation(backButton);
    }

    private void OnBackButtonClicked()
    {
        // Optional: Add fade out or transition effect
        StartCoroutine(TransitionToWelcome());
    }

    private IEnumerator TransitionToWelcome()
    {
        // Optional: Add your transition effects here
        yield return new WaitForSeconds(0.1f);

        // Load Welcome scene
        SceneManager.LoadScene("Welcome");
    }


}
