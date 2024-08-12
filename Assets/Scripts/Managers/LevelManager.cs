using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] List<string> levels = new List<string>();
    [SerializeField] bool loadSceneDirectly = false;
    string levelKey = "levelNo";

    SceneTransition sceneTransition;
    private void Awake()
    {
        Instance = this;

        sceneTransition = GetComponent<SceneTransition>();

        if (loadSceneDirectly)
            LoadCurrentLevel();
    }
    public void LoadCurrentLevel()
    {
        sceneTransition.LoadScene(levels[PlayerPrefs.GetInt(levelKey) % levels.Count]);
    }
    public void LoadNextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt(levelKey);
        int nextLevel = currentLevel + 1;

        PlayerPrefs.SetInt(levelKey, nextLevel);
        PlayerPrefs.Save();

        sceneTransition.LoadScene(levels[PlayerPrefs.GetInt(levelKey) % levels.Count]);

    }
    public void ReloadLevel()
    {
        if (PlayerPrefs.GetInt(levelKey) < levels.Count)
            sceneTransition.LoadScene(levels[PlayerPrefs.GetInt(levelKey)]);
    }
}
