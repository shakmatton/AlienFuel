using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Nomes das cenas")]
    [SerializeField] private string menuSceneName = "StartScreen";
    [SerializeField] private string gameSceneName = "AlienWorld";

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}