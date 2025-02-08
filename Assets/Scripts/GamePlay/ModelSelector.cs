using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class ModelSelector : MonoBehaviour
{
    [SerializeField] public TMP_Dropdown modelDropdown;
    private LLMService llmService;
    private List<string> availableModels = new List<string>();

    private async void Start()
    {
        llmService = FindObjectOfType<LLMService>();
        await FetchAndInitializeModels();
    }

    private async Task FetchAndInitializeModels()
    {
        if (llmService != null)
        {
            // Show loading state
            modelDropdown.ClearOptions();
            modelDropdown.AddOptions(new List<string> { "Loading models..." });
            modelDropdown.interactable = false;

            // Fetch models
            availableModels = await llmService.GetAvailableModels();

            // Update dropdown
            modelDropdown.ClearOptions();
            modelDropdown.AddOptions(availableModels);
            modelDropdown.interactable = true;
            modelDropdown.onValueChanged.AddListener(OnModelSelected);

            // Set initial model
            if (availableModels.Count > 0)
            {
                OnModelSelected(0);
            }
        }
        else
        {
            Debug.LogError("LLMService not found!");
            modelDropdown.ClearOptions();
            modelDropdown.AddOptions(new List<string> { "Error loading models" });
            modelDropdown.interactable = false;
        }
    }

    private void OnModelSelected(int index)
    {
        if (llmService != null && index < availableModels.Count)
        {
            llmService.SetModel(availableModels[index]);
        }
    }

    public void SetDropdown(TMP_Dropdown dropdown)
    {
        modelDropdown = dropdown;
    }
}
