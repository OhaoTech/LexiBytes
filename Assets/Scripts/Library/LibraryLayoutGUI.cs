using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class LibraryLayoutGUI : MonoBehaviour
{
    private List<GameData> games = new List<GameData>();
    private Vector2 scrollPosition;
    private float padding = 20f;
    private float cardWidth = 300f;
    private float cardHeight = 180f;
    private float cardSpacing = 20f;
    private int cardsPerRow = 3;
    private bool stylesInitialized = false;


    // Styles cache
    [System.NonSerialized] private GUIStyle titleStyle;
    [System.NonSerialized] private GUIStyle headerStyle;
    [System.NonSerialized] private GUIStyle cardStyle;
    [System.NonSerialized] private GUIStyle cardTitleStyle;
    [System.NonSerialized] private GUIStyle cardDescStyle;
    [System.NonSerialized] private GUIStyle cardInfoStyle;
    [System.NonSerialized] private GUIStyle backButtonStyle;
    [System.NonSerialized] private GUIStyle newCardStyle;


    private void Awake()
    {
        // Ensure we're not accidentally kept between scenes
        if (transform.parent == null)
        {
            var root = GameObject.Find("LibraryUI");
            if (root != null && root != gameObject)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Initialize your stuff here
        games = new List<GameData>();
    }

    private void Start()
    {
        if (GameLibrary.Instance == null)
        {
            Debug.LogError("GameLibrary not found in scene! Creating one...");
            GameObject libraryObj = new GameObject("GameLibrary");
            libraryObj.AddComponent<GameLibrary>();
        }

        // Load games
        games = GameLibrary.Instance.GetAllGames();
        SortGames();
    }

    private void SortGames()
    {
        // Sort by creation date (latest first)
        games.Sort((a, b) => b.createdAt.CompareTo(a.createdAt));
    }

    private void InitializeStyles()
    {
        // Create base styles first
        var baseLabel = new GUIStyle(GUI.skin.label);
        var baseBox = new GUIStyle(GUI.skin.box);
        var baseButton = new GUIStyle(GUI.skin.button);

        // Title style
        titleStyle = new GUIStyle(baseLabel)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        titleStyle.normal.textColor = Color.white;

        // Header style
        headerStyle = new GUIStyle(baseBox)
        {
            padding = new RectOffset(20, 20, 15, 15),
        };

        // Card style
        cardStyle = new GUIStyle(baseBox)
        {
            padding = new RectOffset(15, 15, 15, 15),
        };
        cardStyle.normal.background = CreateColorTexture(new Color(0.15f, 0.15f, 0.17f, 0.95f));

        // New Card style
        newCardStyle = new GUIStyle(cardStyle);
        newCardStyle.normal.background = CreateColorTexture(new Color(0.2f, 0.2f, 0.22f, 0.95f));

        // Card title style
        cardTitleStyle = new GUIStyle(baseLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true
        };
        cardTitleStyle.normal.textColor = Color.white;

        // Card description style
        cardDescStyle = new GUIStyle(baseLabel)
        {
            fontSize = 14,
            wordWrap = true
        };
        cardDescStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        // Card info style
        cardInfoStyle = new GUIStyle(baseLabel)
        {
            fontSize = 12,
            alignment = TextAnchor.LowerLeft
        };
        cardInfoStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        // Back button style
        backButtonStyle = new GUIStyle(baseButton)
        {
            fontSize = 14,
            padding = new RectOffset(15, 15, 8, 8)
        };
    }

    private void OnGUI()
    {
        if (!stylesInitialized)
        {
            InitializeStyles();
            stylesInitialized = true;
        }

        // Draw background
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, 80));
        using (new GUILayout.HorizontalScope(headerStyle ?? GUI.skin.box))
        {
            // Back button with Event handling
            if (GUILayout.Button("← Back", backButtonStyle ?? GUI.skin.button, GUILayout.Width(80), GUILayout.Height(40)))
            {
                // Handle the click immediately
                HandleBackButton();
            }

            GUILayout.Label("Game Library", titleStyle ?? GUI.skin.label);
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndArea();

        // Calculate grid layout
        float startY = 100f;
        float contentWidth = (cardWidth + cardSpacing) * cardsPerRow - cardSpacing;
        float startX = (Screen.width - contentWidth) / 2;

        // Begin scroll view
        Rect viewRect = new Rect(0, startY, Screen.width, Screen.height - startY);
        float contentHeight = Mathf.Ceil((games.Count + 1) / (float)cardsPerRow) * (cardHeight + cardSpacing);
        Rect contentRect = new Rect(0, 0, Screen.width, contentHeight);

        scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, contentRect);

        // Draw "New Story" card first
        DrawNewStoryCard(startX, 0);

        // Draw game cards
        for (int i = 0; i < games.Count; i++)
        {
            int row = (i + 1) / cardsPerRow;
            int col = (i + 1) % cardsPerRow;
            float x = startX + col * (cardWidth + cardSpacing);
            float y = row * (cardHeight + cardSpacing);

            DrawGameCard(games[i], x, y);
        }

        GUI.EndScrollView();
    }

    private void HandleBackButton()
    {
        // Disable this script
        this.enabled = false;
        // Clear any resources
        if (cardStyle != null) cardStyle.normal.background = null;
        if (newCardStyle != null) newCardStyle.normal.background = null;

        // Load scene
        StartCoroutine(LoadWelcomeScene());
    }

    private IEnumerator LoadWelcomeScene()
    {
        // Optional: Add fade effect
        yield return new WaitForSeconds(0.1f);

        // Force a GC collection to clean up resources
        System.GC.Collect();

        SceneManager.LoadScene("Welcome");
    }

    private void DrawNewStoryCard(float x, float y)
    {
        Rect cardRect = new Rect(x, y, cardWidth, cardHeight);
        GUI.Box(cardRect, "", newCardStyle ?? GUI.skin.box);

        if (GUI.Button(cardRect, ""))
        {
            CreateNewStory();
        }

        // Draw content
        GUILayout.BeginArea(cardRect);
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Label("+ Create New Story", cardTitleStyle ?? GUI.skin.label);
            GUILayout.Label("Start a new adventure...", cardDescStyle ?? GUI.skin.label);
        }
        GUILayout.EndArea();
    }

    private void DrawGameCard(GameData game, float x, float y)
    {
        Rect cardRect = new Rect(x, y, cardWidth, cardHeight);
        GUI.Box(cardRect, "", cardStyle ?? GUI.skin.box);

        Rect deleteRect = new Rect(cardRect.xMax - 30, cardRect.y + 5, 25, 25);
        if (GUI.Button(deleteRect, "×"))
        {
            DeleteGameCard(game);
            return;
        }

        if (GUI.Button(cardRect, ""))
        {
            LoadGame(game);
        }

        // Draw content
        GUILayout.BeginArea(cardRect);
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Label(game.title, cardTitleStyle ?? GUI.skin.label);
            GUILayout.Label(game.description, cardDescStyle ?? GUI.skin.label);

            GUILayout.FlexibleSpace();

            GUILayout.Label($"Model: {game.modelName}", cardInfoStyle ?? GUI.skin.label);
            GUILayout.Label($"Last played: {game.lastPlayedAt:g}", cardInfoStyle ?? GUI.skin.label);
        }
        GUILayout.EndArea();
    }

    private void LoadGame(GameData game)
    {
        CleanupAndTransition(() =>
            {
                PlayerPrefs.SetString("SelectedGameId", game.id);
                SceneManager.LoadScene("Gameplay");
            });
    }

    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private void CreateNewStory()
    {
        // Disable and cleanup this GUI before loading new scene
        CleanupAndTransition(() =>
        {
            PlayerPrefs.DeleteKey("SelectedGameId");
            SceneManager.LoadScene("Gameplay");
        });
    }

    private void CleanupAndTransition(System.Action onComplete)
    {
        // Disable this script
        this.enabled = false;

        // Clear any resources
        if (cardStyle != null) cardStyle.normal.background = null;
        if (newCardStyle != null) newCardStyle.normal.background = null;

        // Clear styles
        titleStyle = null;
        headerStyle = null;
        cardStyle = null;
        cardTitleStyle = null;
        cardDescStyle = null;
        cardInfoStyle = null;
        backButtonStyle = null;
        newCardStyle = null;

        // Reset initialization flag
        stylesInitialized = false;

        // Clear game data references
        games.Clear();

        // Force cleanup
        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        // Destroy this GameObject
        StartCoroutine(TransitionToScene(onComplete));
    }

    private IEnumerator TransitionToScene(System.Action onComplete)
    {
        // Optional: Add fade effect
        yield return new WaitForSeconds(0.1f);

        // Execute the scene transition
        onComplete?.Invoke();

        // Destroy this GameObject
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clear any resources
        if (cardStyle != null) cardStyle.normal.background = null;
        if (newCardStyle != null) newCardStyle.normal.background = null;

        // Clear styles
        titleStyle = null;
        headerStyle = null;
        cardStyle = null;
        cardTitleStyle = null;
        cardDescStyle = null;
        cardInfoStyle = null;
        backButtonStyle = null;
        newCardStyle = null;

        // Clear game data references
        games.Clear();

        // Force cleanup
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }

    private void CreateNewGameCard()
    {
        var newGame = GameLibrary.Instance.CreateGame();
        LoadGame(newGame); // Automatically open the new game
    }

    private void DeleteGameCard(GameData game)
    {
        GameLibrary.Instance.DeleteGame(game.id);
        RefreshGameList();
    }

    private void RefreshGameList()
    {
        games = GameLibrary.Instance.GetAllGames();
        SortGames();
    }

    // Subscribe to GameLibrary events
    private void OnEnable()
    {
        StartCoroutine(SubscribeToEvents());

    }

    private void OnDisable()
    {
        if (GameLibrary.Instance != null)
        {
            GameLibrary.Instance.OnGameCreated -= OnGameCreated;
            GameLibrary.Instance.OnGameUpdated -= OnGameUpdated;
            GameLibrary.Instance.OnGameDeleted -= OnGameDeleted;
        }
    }

    private IEnumerator SubscribeToEvents()
    {
        // Wait until GameLibrary is available
        while (GameLibrary.Instance == null)
        {
            yield return null;
        }

        // Now it's safe to subscribe
        GameLibrary.Instance.OnGameCreated += OnGameCreated;
        GameLibrary.Instance.OnGameUpdated += OnGameUpdated;
        GameLibrary.Instance.OnGameDeleted += OnGameDeleted;
    }

    private void OnGameCreated(GameData game)
    {
        RefreshGameList();
    }

    private void OnGameUpdated(GameData game)
    {
        RefreshGameList();
    }

    private void OnGameDeleted(string gameId)
    {
        RefreshGameList();
    }

}
