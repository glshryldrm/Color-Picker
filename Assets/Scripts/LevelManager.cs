using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<string> levels = new List<string>();
    public Animator transition;
    string levelKey = "currentLevel";
    void ReloaadLevelPrivate()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReloadLevel()
    {
        Invoke(nameof(ReloaadLevelPrivate), 0.5f);
    }
    void LoadNextLevelPrivate()
    {

        int currentLevel = PlayerPrefs.GetInt(levelKey);
        int nextLevel = currentLevel + 1;
        LoadLevel(nextLevel);
    }
    public void LoadNextLevel()
    {
        Invoke(nameof(LoadNextLevelPrivate), 0.5f);
    }
    public void LoadLevel(int level)
    {
        SaveCurrentLevel(levelKey, level);

        if (level >= levels.Count)
        {
            int randomLevel = Random.Range(0, levels.Count);

            SceneManager.LoadScene(levels[randomLevel]);
            transition.SetBool("isLoaded", true);
        }
        else
        {
            SceneManager.LoadScene(levels[level]);
            transition.SetBool("isLoaded", true);
        }
        transition.SetBool("isLoaded", false);
    }
    void SaveCurrentLevel(string key, int level)
    {
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }
}
