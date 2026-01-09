using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnStartGamePressed()
    {
        // Load the Game Loop Scene
        SceneManager.LoadScene(2);
    }
}