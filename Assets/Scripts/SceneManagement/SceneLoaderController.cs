using UnityEngine;

public class SceneLoaderController : MonoBehaviour
{
    public static void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
