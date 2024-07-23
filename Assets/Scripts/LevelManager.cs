using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    int sceneIndex, levelPassed; // bu deðiþkenleri kullanma
    public List<string> levels = new List<string>();
    public Animator transition;
    [SerializeField] bool loadLevelOnStart = false;

    private void Start()
    {
        levelPassed = PlayerPrefs.GetInt("LevelPassed", 0); // current level adýnda bir deðiþende  level key oluþtur
        sceneIndex = levelPassed;
        if (loadLevelOnStart)
        {
            LoadSavedLevel();
        }

    }
    public void ReloadLevel()
    {
        StartCoroutine(LoadLevel(sceneIndex));
    }

    public void LoadNextLevel()
    {
        sceneIndex++;

        if (sceneIndex >= levels.Count)
        {
            int randomIndex = Random.Range(0, levels.Count);
            StartCoroutine(LoadLevel(randomIndex));
        }
        else
        {
            StartCoroutine(LoadLevel(sceneIndex));
            PlayerPrefs.SetInt("LevelPassed", sceneIndex);
        }
    }

    public IEnumerator LoadLevel(int sceneIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(levels[sceneIndex]);
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
}
