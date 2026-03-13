using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene loading

public class SceneLoader : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject lessonPanel;

    // Call this method from your UI Button
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartPanelDeactive()
    {
        startPanel.SetActive(false);
    }

   public void LessonPanelActive()
    {
        lessonPanel.SetActive(true);
    } 
}


