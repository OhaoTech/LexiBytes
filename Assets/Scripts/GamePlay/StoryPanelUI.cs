using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class StoryPanelUI : MonoBehaviour
{
    public ScrollRect scrollRect { get; private set; }
    public TextMeshProUGUI storyText { get; private set; }

    [Header("Style")]
    public Color panelColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);
    public Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public float cornerRadius = 8f;
    public float minHeight = 300f;
    public float padding = 16f;

    [Header("Scrollbar Style")]
    public float scrollbarWidth = 8f;
    public float scrollbarMinAlpha = 0f;
    public float scrollbarMaxAlpha = 0.8f;
    public Color scrollbarColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color scrollbarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    public float fadeSpeed = 0.3f;
    public float hideDelay = 1.5f;

    private Scrollbar scrollbar;
    private Image scrollbarImage;
    private Image handleImage;
    private float lastScrollTime;
    private bool isScrollbarVisible = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        SetupStoryPanel();
    }

    private void SetupStoryPanel()
    {
        // Panel setup
        var panelImage = gameObject.AddComponent<Image>();
        panelImage.color = panelColor;
        panelImage.sprite = CreateRoundedRectSprite(cornerRadius);
        panelImage.type = Image.Type.Sliced;

        // Layout settings
        var storyLayout = gameObject.AddComponent<LayoutElement>();
        storyLayout.flexibleHeight = 1;
        storyLayout.minHeight = minHeight;

        // Scroll view setup
        scrollRect = gameObject.AddComponent<ScrollRect>();

        // Viewport
        var viewport = CreateViewport();
        var content = CreateContent(viewport.transform);
        storyText = CreateStoryText(content.transform);

        // Setup scroll view references
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 20f;

        CreateScrollbar();
    }

    private GameObject CreateViewport()
    {
        var viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(transform);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.sprite = CreateRoundedRectSprite(cornerRadius);

        var viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = Vector2.zero;
        viewportRT.localPosition = Vector3.zero;

        return viewport;
    }

    private GameObject CreateContent(Transform parent)
    {
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(parent);

        var contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        contentLayout.childAlignment = TextAnchor.UpperLeft;

        var contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Ensure proper anchoring and sizing
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

        return content;
    }

    private TextMeshProUGUI CreateStoryText(Transform parent)
    {
        var textObj = new GameObject("StoryText", typeof(RectTransform));
        textObj.transform.SetParent(parent);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.color = textColor;
        text.fontSize = 18;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.Top;
        text.richText = true;
        text.margin = new Vector4(padding, 0, padding, 0);

        var textRT = textObj.GetComponent<RectTransform>();
        textRT.localPosition = Vector3.zero;
        textRT.localRotation = Quaternion.identity;
        textRT.localScale = Vector3.one;

        return text;
    }

    private void CreateScrollbar()
    {
        var scrollbarObj = new GameObject("Scrollbar", typeof(RectTransform));
        scrollbarObj.transform.SetParent(transform);
        scrollbar = scrollbarObj.AddComponent<Scrollbar>();

        // Scrollbar background
        scrollbarImage = scrollbarObj.AddComponent<Image>();
        scrollbarImage.sprite = CreateRoundedRectSprite(cornerRadius);
        scrollbarImage.type = Image.Type.Sliced;
        scrollbarImage.color = new Color(
            scrollbarBackgroundColor.r,
            scrollbarBackgroundColor.g,
            scrollbarBackgroundColor.b,
            scrollbarMinAlpha * 0.5f
        );

        // Scrollbar handle
        var handleObj = new GameObject("Handle", typeof(RectTransform));
        handleObj.transform.SetParent(scrollbarObj.transform);
        handleImage = handleObj.AddComponent<Image>();
        handleImage.sprite = CreateRoundedRectSprite(cornerRadius);
        handleImage.type = Image.Type.Sliced;
        handleImage.color = new Color(
            scrollbarColor.r,
            scrollbarColor.g,
            scrollbarColor.b,
            scrollbarMinAlpha
        );

        // Setup scrollbar
        scrollbar.handleRect = handleObj.GetComponent<RectTransform>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        // Position scrollbar
        var scrollbarRT = scrollbarObj.GetComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(1, 0);
        scrollbarRT.anchorMax = new Vector2(1, 1);
        scrollbarRT.pivot = new Vector2(1, 1);
        scrollbarRT.sizeDelta = new Vector2(scrollbarWidth, 0);
        scrollbarRT.anchoredPosition = new Vector2(0, 0);

        // Setup handle rect transform
        var handleRT = handleObj.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(1, 1);
        handleRT.sizeDelta = new Vector2(0, 0);

        // Connect to scroll rect
        scrollRect.verticalScrollbar = scrollbar;

        // Add scroll listener
        scrollRect.onValueChanged.AddListener(OnScroll);

        // Initialize visibility
        isScrollbarVisible = false;
    }

    private void OnScroll(Vector2 position)
    {
        lastScrollTime = Time.time;
        ShowScrollbar();
    }

    private void ShowScrollbar()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeScrollbar(true));
    }

    private void Update()
    {
        if (isScrollbarVisible && Time.time > lastScrollTime + hideDelay)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeScrollbar(false));
        }
    }

    private IEnumerator FadeScrollbar(bool fadeIn)
    {
        float startAlpha = scrollbarImage.color.a;
        float targetAlpha = fadeIn ? scrollbarMaxAlpha : scrollbarMinAlpha;
        float elapsed = 0f;

        isScrollbarVisible = fadeIn;

        while (elapsed < fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeSpeed;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, SmoothStep(progress));
            SetScrollbarAlpha(currentAlpha);
            yield return null;
        }

        SetScrollbarAlpha(targetAlpha);
    }

    private float SmoothStep(float x)
    {
        return x * x * (3 - 2 * x);
    }

    private void SetScrollbarAlpha(float alpha)
    {
        Color bgColor = scrollbarBackgroundColor;
        Color handleColor = scrollbarColor;

        bgColor.a = alpha * 0.5f;
        handleColor.a = alpha;

        scrollbarImage.color = bgColor;
        handleImage.color = handleColor;
    }

    private Sprite CreateHoverEffect(Sprite original)
    {
        // Create a slightly larger version of the sprite for hover effect
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 64, 64),
            new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.Tight, new Vector4(cornerRadius * 1.2f, cornerRadius * 1.2f, cornerRadius * 1.2f, cornerRadius * 1.2f));
    }

    // Add mouse hover effect
    private void OnEnable()
    {
        if (scrollRect != null)
        {
            var eventTrigger = scrollRect.gameObject.AddComponent<EventTrigger>();

            var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((data) => ShowScrollbar());
            eventTrigger.triggers.Add(pointerEnter);
        }
    }

    private Sprite CreateRoundedRectSprite(float radius)
    {
        var texture = new Texture2D(32, 32);
        var colors = new Color[1024];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 32, 32),
            new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.Tight, new Vector4(radius, radius, radius, radius));
    }
}
