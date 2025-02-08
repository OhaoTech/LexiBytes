using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Collections.Generic;


public class LLMService : MonoBehaviour
{
    private const string OLLAMA_URL = "http://localhost:11434/api/generate";
    private const string OLLAMA_LIST_URL = "http://localhost:11434/api/tags";
    private string currentModel;

    [Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream = false;
    }

    [Serializable]
    private class OllamaStreamResponse
    {
        public string response;
        public bool done;
    }

    [Serializable]
    private class ModelInfo
    {
        public string name;
        public string modified_at;
        public string size;
    }

    [Serializable]
    private class ModelListResponse
    {
        public List<ModelInfo> models;
    }

    public void SetModel(string modelName)
    {
        currentModel = modelName;
        Debug.Log($"Model set to: {currentModel}");
    }

    public async Task<List<string>> GetAvailableModels()
    {
        try
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(OLLAMA_LIST_URL))
            {
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error fetching models: {webRequest.error}");
                    return new List<string>() { currentModel };
                }

                string jsonResponse = webRequest.downloadHandler.text;
                ModelListResponse response = JsonUtility.FromJson<ModelListResponse>(jsonResponse);
                List<string> modelNames = new List<string>();

                if (response.models != null)
                {
                    foreach (var model in response.models)
                    {
                        string modelName = model.name.Split(':')[0]; // Get name without version
                        modelNames.Add(model.name); // Use full name including version
                    }
                }

                Debug.Log($"Found models: {string.Join(", ", modelNames)}");
                return modelNames;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception fetching models: {e.Message}");
            return new List<string>() { currentModel };
        }
    }

    public async Task StreamResponse(string prompt, Action<string> onToken, Action onComplete)
    {
        try
        {
            OllamaRequest request = new OllamaRequest
            {
                model = currentModel,
                prompt = prompt,
                stream = true  // Enable streaming
            };

            string jsonRequest = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

            using (UnityWebRequest webRequest = new UnityWebRequest(OLLAMA_URL, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                var operation = webRequest.SendWebRequest();

                string buffer = "";
                while (!operation.isDone)
                {
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        string newText = webRequest.downloadHandler.text.Substring(buffer.Length);
                        buffer = webRequest.downloadHandler.text;

                        // Split by newlines as Ollama sends one JSON object per line
                        string[] chunks = newText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var chunk in chunks)
                        {
                            try
                            {
                                var response = JsonUtility.FromJson<OllamaStreamResponse>(chunk);
                                if (response.response != null)
                                {
                                    onToken?.Invoke(response.response);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error parsing chunk: {e.Message}");
                            }
                        }
                    }
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    onToken?.Invoke("\nError: Failed to get response from the model.");
                }

                onComplete?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
            onToken?.Invoke("\nError: Something went wrong.");
            onComplete?.Invoke();
        }
    }
}
