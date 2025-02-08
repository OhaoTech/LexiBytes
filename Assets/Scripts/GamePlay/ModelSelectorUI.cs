using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModelSelectorUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown dropdown;
    public TextMeshProUGUI label;

    [Header("Style")]
    public Color backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
    public Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public float cornerRadius = 8f;
    public float height = 50f;

    private void Awake()
    {
        // Add layout element to the selector panel itself
        var selectorLayout = gameObject.AddComponent<LayoutElement>();
        selectorLayout.minHeight = height;
        selectorLayout.flexibleHeight = 0; // Prevent vertical stretching

        if (!TryGetComponent<HorizontalLayoutGroup>(out _))
        {
            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }

        // Add ModelSelector component
        if (!TryGetComponent<ModelSelector>(out var modelSelector))
        {
            modelSelector = gameObject.AddComponent<ModelSelector>();
        }

        SetupUI();
    }

    private void SetupUI()
    {
        // Setup label if not assigned
        if (label == null)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(transform);
            label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "Model:";
            label.color = textColor;
            label.fontSize = 16;
            label.alignment = TextAlignmentOptions.Right;

            // ContentSizeFitter to auto-fit width
            var labelFitter = labelObj.AddComponent<ContentSizeFitter>();
            labelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            labelFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // For height control only
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.minHeight = height;
            labelLayout.minWidth = 80;
            labelLayout.flexibleWidth = 0; // Prevent stretching

            // Padding to the label
            label.margin = new Vector4(20, 0, 8, 0);
        }

        if (TryGetComponent<HorizontalLayoutGroup>(out var layout))
        {
            layout.padding = new RectOffset(36, 16, 8, 8);
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = false;
        }

        // Setup dropdown if not assigned
        if (dropdown == null)
        {
            var dropdownObj = new GameObject("ModelDropdown", typeof(RectTransform));
            dropdownObj.transform.SetParent(transform);
            dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Style the dropdown
            var dropdownImage = dropdownObj.AddComponent<Image>();
            dropdownImage.color = backgroundColor;

            // Create Template
            var templateObj = new GameObject("Template");
            templateObj.SetActive(false);
            templateObj.transform.SetParent(dropdownObj.transform);
            var templateRT = templateObj.AddComponent<RectTransform>();
            var templateImage = templateObj.AddComponent<Image>();
            templateImage.color = backgroundColor;

            // Create Viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform);
            var viewportRT = viewportObj.AddComponent<RectTransform>();
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = backgroundColor;
            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Create Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform);
            var contentRT = contentObj.AddComponent<RectTransform>();
            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandHeight = false;
            var contentFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create Item Template
            var itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform);
            var itemRT = itemObj.AddComponent<RectTransform>();
            var itemToggle = itemObj.AddComponent<Toggle>();
            var itemImage = itemObj.AddComponent<Image>();
            itemImage.color = backgroundColor;

            // Create Item Label
            var itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform);
            var itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabel.color = textColor;
            itemLabel.fontSize = 14;
            itemLabel.alignment = TextAlignmentOptions.Left;

            // Create Caption
            var captionObj = new GameObject("Caption");
            captionObj.transform.SetParent(dropdownObj.transform);
            var captionText = captionObj.AddComponent<TextMeshProUGUI>();
            captionText.color = textColor;
            captionText.fontSize = 14;
            captionText.alignment = TextAlignmentOptions.Left;

            // Setup RectTransforms
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.sizeDelta = new Vector2(0, 150);

            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;

            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = Vector2.zero;

            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 30);

            var itemLabelRT = itemLabelObj.GetComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.sizeDelta = new Vector2(-20, -10);
            itemLabelRT.anchoredPosition = new Vector2(10, 0);

            var captionRT = captionObj.GetComponent<RectTransform>();
            captionRT.anchorMin = Vector2.zero;
            captionRT.anchorMax = Vector2.one;
            captionRT.sizeDelta = new Vector2(-20, -10);
            captionRT.anchoredPosition = new Vector2(10, 0);

            // Setup Dropdown references
            dropdown.template = templateRT;
            dropdown.itemText = itemLabel;
            dropdown.captionText = captionText;

            // Add layout element for proper sizing
            var dropdownLayout = dropdownObj.AddComponent<LayoutElement>();
            dropdownLayout.minHeight = height;
            dropdownLayout.minWidth = 200;
        }

        // Connect to ModelSelector
        GetComponent<ModelSelector>().SetDropdown(dropdown);
    }
}
