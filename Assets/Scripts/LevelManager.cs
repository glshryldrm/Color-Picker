using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    public List<string> levels = new List<string>();
    public Animator transition;
    [SerializeField] bool loadLevelOnStart = false;
    string levelKey = "CurrentLevel";
    public void ReloadLevel()
    {
        StartCoroutine(LoadLevel(PlayerPrefs.GetInt(levelKey)));
    }

    public void LoadNextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt(levelKey, 0);
        int nextLevel = currentLevel + 1;
        SaveCurrentLevel(levelKey, nextLevel);
        StartCoroutine(LoadLevel(nextLevel));
    }

    public IEnumerator LoadLevel(int level)
    {
        SaveCurrentLevel(levelKey, level);

        if (level >= levels.Count)
        {
            int randomLevel = Random.Range(0, levels.Count);
            transition.SetTrigger("Start");

            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(levels[randomLevel]);
        }
        else
        {
            transition.SetTrigger("Start");

            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(levels[level]);
        }


    }

    void SaveCurrentLevel(string levelKey, int level)
    {
        PlayerPrefs.SetInt(levelKey, level);
        PlayerPrefs.Save();
    }
}
