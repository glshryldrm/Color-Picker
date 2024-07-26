using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public enum PawnType { player, bot}
    public Vector3 position;
    public Color color;
    public AIManager.Difficulty difficulty;
    public PawnType pawnType;
    public Vector3 targetPosition;
    public bool isMoving = false;
    public int levelPassed = 0;
    public void EliminatePawn()
    {
        // Piyonu yok et veya devre dýþý býrak
        gameObject.SetActive(false);
    }
}
