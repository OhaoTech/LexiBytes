using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class MultiButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum ButtonType
    {
        Start,
        Options,
        Quit
    }

    public ButtonType buttonType;
    private Coroutine punchCoroutine;
    private Coroutine enhanceTextCoroutine;

    [Header("Text Component")]
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Color Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float colorChangeSpeed = 0.5f;
    [SerializeField] private float colorSaturation = 0.8f;
    [SerializeField] private float colorBrightness = 1f;
    [SerializeField] private bool useRainbowEffect = true;

    [Header("Text Effects")]
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private Color outlineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private float textWaveSpeed = 1f;
    [SerializeField] private float textWaveAmount = 3f;

    [Header("Hover Animation")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private float hoverScaleSpeed = 0.1f;

    [Header("Click Animation")]
    [SerializeField] private float clickPunchForce = 0.3f;
    [SerializeField] private float clickElasticity = 0.5f;
    [SerializeField] private int clickVibrato = 10;
    [SerializeField] private Vector3 clickScaleMultiplier = new Vector3(1.2f, 1.2f, 1f);

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private bool isHovering = false;
    private float hueValue = 0f;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool isAnimating = false;

    private void Start()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // Setup text effects
        buttonText.enableVertexGradient = true;
        buttonText.fontMaterial.EnableKeyword("OUTLINE_ON");
        buttonText.outlineWidth = outlineWidth;
        buttonText.outlineColor = outlineColor;

        // Automatically add and setup AudioSource if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Configure AudioSource defaults
                audioSource.playOnAwake = false;
                audioSource.volume = 0.5f;
            }
        }

        originalScale = transform.localScale;
        buttonText.color = defaultColor;
    }

    private void Update()
    {
        if (isHovering && useRainbowEffect)
        {
            UpdateRainbowEffect();
        }
    }

    private void UpdateRainbowEffect()
    {
        // Slower, smoother rainbow effect
        hueValue = (hueValue + Time.deltaTime * colorChangeSpeed) % 1f;

        // Create a more interesting color gradient
        VertexGradient gradient = new VertexGradient();

        // Main color with wave effect
        float wave = Mathf.Sin(Time.time * textWaveSpeed) * 0.1f + 0.9f;
        Color mainColor = Color.HSVToRGB(hueValue, colorSaturation * wave, colorBrightness);

        // Create complementary colors for gradient
        Color topColor = Color.HSVToRGB((hueValue + 0.1f) % 1f, colorSaturation, colorBrightness);
        Color bottomColor = Color.HSVToRGB((hueValue + 0.2f) % 1f, colorSaturation * 0.8f, colorBrightness);

        gradient.topLeft = topColor;
        gradient.topRight = topColor;
        gradient.bottomLeft = bottomColor;
        gradient.bottomRight = bottomColor;

        buttonText.colorGradient = gradient;

        // Add subtle text wave animation
        if (buttonText.transform != null)
        {
            float waveY = Mathf.Sin(Time.time * textWaveSpeed) * textWaveAmount;
            Vector3 textPos = buttonText.transform.localPosition;
            textPos.y = waveY;
            buttonText.transform.localPosition = textPos;
        }
    }

    private IEnumerator EnhanceTextOnHover()
    {
        float originalOutlineWidth = buttonText.outlineWidth;
        Color originalOutlineColor = buttonText.outlineColor;

        // Enhance outline during hover
        buttonText.outlineWidth = outlineWidth * 1.5f;
        buttonText.outlineColor = new Color(
            outlineColor.r * 1.2f,
            outlineColor.g * 1.2f,
            outlineColor.b * 1.2f,
            outlineColor.a
        );

        while (isHovering)
        {
            // Add subtle pulse to text size
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.02f;
            buttonText.transform.localScale = new Vector3(pulse, pulse, 1f);
            yield return null;
        }

        // Reset effects
        buttonText.outlineWidth = originalOutlineWidth;
        buttonText.outlineColor = originalOutlineColor;
        buttonText.transform.localScale = Vector3.one;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAnimating) return;

        isHovering = true;
        PlaySound(hoverSound);

        StopAllAnimations();
        scaleCoroutine = StartCoroutine(ScaleWithEasing(hoverScale));
        enhanceTextCoroutine = StartCoroutine(EnhanceTextOnHover());
    }

    private void StopAllAnimations()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }
        if (enhanceTextCoroutine != null)
        {
            StopCoroutine(enhanceTextCoroutine);
            enhanceTextCoroutine = null;
        }

        isAnimating = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Stop all running coroutines
        StopAllAnimations();

        // Reset to original state
        scaleCoroutine = StartCoroutine(ScaleWithEasing(originalScale));
        StartCoroutine(ResetColorWithEasing());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;
        StartCoroutine(HandleClick());
    }

    private IEnumerator HandleClick()
    {
        isAnimating = true;
        PlaySound(clickSound);

        punchCoroutine = StartCoroutine(PunchScale());

        yield return new WaitForSeconds(0.3f);

        switch (buttonType)
        {
            case ButtonType.Start:
                Debug.Log("Start button clicked");
                break;
            case ButtonType.Options:
                Debug.Log("Options button clicked");
                break;
            case ButtonType.Quit:
                Debug.Log("Quit button clicked");
                QuitGame();
                break;
            default:
                Debug.LogError("Unknown button type");
                break;
        }

        isAnimating = false;
    }

    private void OnDisable()
    {
        StopAllAnimations();
        transform.localScale = originalScale;
        if (buttonText != null)
        {
            buttonText.color = defaultColor;
            buttonText.transform.localScale = Vector3.one;
            buttonText.outlineWidth = outlineWidth;
            buttonText.outlineColor = outlineColor;
        }
    }

    private IEnumerator PunchScale()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.Scale(startScale, clickScaleMultiplier);

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Elastic easing
            float elasticProgress = ElasticEaseOut(progress);

            // Apply wobble effect
            float wobble = Mathf.Sin(progress * clickVibrato) * clickPunchForce * (1 - progress);
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, elasticProgress);
            currentScale += new Vector3(wobble, wobble, 0);

            transform.localScale = currentScale;
            yield return null;
        }

        transform.localScale = startScale;
    }

    private float ElasticEaseOut(float t)
    {
        float p = clickElasticity;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
    }

    private IEnumerator ScaleWithEasing(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < hoverScaleSpeed)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / hoverScaleSpeed;

            // Smooth step for more natural easing
            float smoothProgress = progress * progress * (3f - 2f * progress);

            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator ResetColorWithEasing()
    {
        Color startColor = buttonText.color;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Smooth step for color transition
            float smoothProgress = progress * progress * (3f - 2f * progress);

            buttonText.color = Color.Lerp(startColor, defaultColor, smoothProgress);
            yield return null;
        }

        buttonText.color = defaultColor;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
