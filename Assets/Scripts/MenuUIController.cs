using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject aboutPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        storyPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        aboutPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowStory()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(true);
        instructionsPanel.SetActive(false);
        aboutPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ShowInstructions()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        aboutPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ShowAbout()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        aboutPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}