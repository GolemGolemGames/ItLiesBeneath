using UnityEngine;

public class PersistantSceneController : MonoBehaviour
{
    [SerializeField] private string _nextSceneName;

    void Start()
    {
        SceneLoaderController.LoadScene(_nextSceneName);
    }
}
