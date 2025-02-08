using UnityEngine;

public class MultiButtonHandler : MonoBehaviour
{
    // Enum to define different button types
    public enum ButtonType
    {
        Start,
        Options,
        Quit
    }

    public ButtonType buttonType;

    // Method to handle the click event
    public void OnButtonClick()
    {
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
    }

    // Separate method for quitting the game to keep the code organized
    private void QuitGame()
    {
        // Unity's method to quit the application
        Application.Quit();
        
        // This is for Unity Editor, it stops the game in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}