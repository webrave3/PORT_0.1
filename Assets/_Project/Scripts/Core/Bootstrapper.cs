using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject _systemsPrefab; // We will create this next

    private void Start()
    {
        // 1. Check if GameManager exists, if not, spawn the Systems
        if (GameManager.Instance == null && _systemsPrefab != null)
        {
            Instantiate(_systemsPrefab);
        }

        // 2. Load the Main Menu
        SceneManager.LoadScene(1); // Index 1 is MainMenu
    }
}