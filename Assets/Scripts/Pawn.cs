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
    public GridCell targetGrid;
    public bool isMoving = false;
    public int levelPassed = 0;
    public bool isSuccess = true;
    public float similarity;
}
