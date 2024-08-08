using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pawn : MonoBehaviour
{
    public enum PawnType { player, bot}
    public Vector3 position;
    public Color color;
    public PawnType pawnType;
    public GridCell targetGrid;
    public bool isSuccess = true;
    public float similarity;
    public BotPawns botPawns;
}
