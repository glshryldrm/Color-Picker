using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    int sceneIndex, levelPassed;
    public List<string> levels = new List<string>();
    public Animator transition;

    void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        levelPassed = PlayerPrefs.GetInt("LevelPassed", 0);

        if (sceneIndex == 0)
        {
            LoadSavedLevel();
        }
    }

    void LoadSavedLevel()
    {
        if (levelPassed < levels.Count)
        {
            SceneManager.LoadScene(levels[levelPassed]);
        }
        else
        {
            Debug.LogError("Level index out of bounds");
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        if (sceneIndex < levels.Count - 1)
        {
            StartCoroutine(LoadLevel(sceneIndex + 1));
            //SceneManager.LoadScene(levels[sceneIndex + 1]);
            PlayerPrefs.SetInt("LevelPassed", sceneIndex + 1);
        }
        else
        {
            int randomIndex = Random.Range(0, levels.Count);
            StartCoroutine(LoadLevel(randomIndex));
            //SceneManager.LoadScene(levels[randomIndex]);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSpecificLevel(int levelIndex)
    {
        if (levelIndex < levels.Count)
        {
            SceneManager.LoadScene(levels[levelIndex]);
            PlayerPrefs.SetInt("LevelPassed", levelIndex);
        }
        else
        {
            Debug.LogError("Level index out of bounds");
        }
    }
    public IEnumerator LoadLevel(int sceneIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(levels[sceneIndex]);
    }
}
