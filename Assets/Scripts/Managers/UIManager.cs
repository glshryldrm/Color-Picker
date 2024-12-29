using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject ScorePanel; 
    public static bool check = false;
    string levelKey = "currentLevel";
    static int i = 0;
    public static UIManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        changeName();
    }
    void changeName()
    {
        if (check)
        {
            if (levelText != null)
                levelText.text =(LevelManager.Instance.levels[i]).ToString();
            i++;
            check = false;
        }
        else
        {
             if (levelText != null)
                levelText.text =(LevelManager.Instance.levels[i]).ToString();
            i++;
        }
    }
    public void ActivateScorePanel(bool check)
    {
        ScorePanel.SetActive(check);
    }
}
