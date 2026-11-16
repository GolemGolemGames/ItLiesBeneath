using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    private static Singleton<T> _instance;

    public static Singleton<T> Instance { get => _instance; set => _instance = value; }

    private void Awake()
    {
        CheckSingleton();
    }

    private void CheckSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
