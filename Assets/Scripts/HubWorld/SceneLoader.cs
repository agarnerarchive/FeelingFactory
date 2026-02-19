using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading

public class SceneLoader : MonoBehaviour
{
    // Call this method from your UI Button
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
