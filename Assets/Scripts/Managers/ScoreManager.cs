using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] GameManager gameManager;
    private void Awake()
    {
        Instance = this;
    }
    public void CalculatePawnsTotalScore()
    {
        foreach (var pawn in gameManager.botPawns)
        {
            pawn.totalScore += pawn.targetGrid.Score;
        }
    }
}
