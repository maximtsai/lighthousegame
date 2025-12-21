using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateDisplay : MonoBehaviour
{
    private static GameStateDisplay instance;
    private TMP_Text text;

    void Awake()
    {
        // Singleton + persistence
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        text = GetComponent<TMP_Text>();
        Refresh();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameState.OnDataChanged += Refresh;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameState.OnDataChanged -= Refresh;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (text == null) return;
        text.text = GameState.StringifyData();
    }
}
