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
        if (loadLevelOnStart)
        {
            StartCoroutine(LoadLevel(levelPassed)); // playerPref kaydetmek için fonksiyon oluþtur
        }

    }
    public void ReloadLevel()
    {
        StartCoroutine(LoadLevel(sceneIndex));
    }

    public void LoadNextLevel()
    {
        if (sceneIndex >= levels.Count)
        {
            int randomIndex = Random.Range(0, levels.Count);
            StartCoroutine(LoadLevel(randomIndex));
            //SceneManager.LoadScene(levels[randomIndex])
        }
        else
        {
            StartCoroutine(LoadLevel(sceneIndex + 1));
            //SceneManager.LoadScene(levels[sceneIndex + 1]);
            PlayerPrefs.SetInt("LevelPassed", sceneIndex + 1);
        }
    }
    public IEnumerator LoadLevel(int sceneIndex) // bu fonksiyon üzerinde deðiþiklik yapma
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
