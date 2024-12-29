using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] GameManager gameManager;
    private Dictionary<string, int> playerScores = new Dictionary<string, int>();
    public TextMeshProUGUI scoreText;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        AddScore();
    }
    public void CalculatePawnsTotalScore()
    {
        foreach (var pawn in gameManager.botPawns)
        {
            pawn.totalScore += pawn.targetGrid.Score;

        }
        
    }
    public void AddScore()
    {
        playerScores.Clear();
        for (int i = 0; i < gameManager.botPawns.Count; i++)
        {
            if (gameManager.botPawns[i].pawnType == Pawn.PawnType.player)
                playerScores.Add("YOU " , gameManager.botPawns[i].totalScore);
            else
            playerScores.Add("PLAYER " + (i), gameManager.botPawns[i].totalScore);
        }
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "";
        foreach (var player in playerScores)
        {
            scoreText.text += $"{player.Key}: {player.Value}\n";
        }
    }

}
