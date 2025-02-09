using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;
    public static SceneLoader Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Library")
        {
            SetupLibraryScene();
        }
        else
        {
            // Cleanup any remaining LibraryLayoutGUI when loading other scenes
            var libraryUI = FindObjectOfType<LibraryLayoutGUI>();
            if (libraryUI != null)
            {
                Destroy(libraryUI.gameObject);
            }
        }
    }

    private void SetupLibraryScene()
    {
        // Create a GameObject for the Library UI
        GameObject libraryUI = new GameObject("LibraryUI");
        libraryUI.AddComponent<LibraryLayoutGUI>();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
