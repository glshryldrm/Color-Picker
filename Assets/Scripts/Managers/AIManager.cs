using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIManager : MonoBehaviour
{
    public List<BotPawns> pawns = new List<BotPawns>();
    public GameManager gameManager;
    public ColorManager colorManager;
    public float moveSpeed = 5f;
    public float moveDuration = 1f; // Pawn hareket süresi
    public float delayBetweenPawns = 0.2f; // Pawn hareketleri arasındaki gecikme
    public float jumpPower = 1f; // Pawn'ın yukarı zıplama gücü
    public int numJumps = 1; // Pawn'ın zıplama sayısı
    public int levelDiffuculty = 60;
    public void DetermineBotTargets()
    {
        List<Pawn> botPawns = new List<Pawn>();
        foreach (Pawn pawn in gameManager.botPawns)
        {
            if (pawn.pawnType == Pawn.PawnType.bot)
            {
                botPawns.Add(pawn);
            }
        }
        foreach (Pawn pawn in botPawns)
        {
            SetNewTarget(pawn);
        }
    }

    void SetNewTarget(Pawn pawn)
    {
        List<GridCell> successTargets;
        List<GridCell> failTargets;
        ClassifyGridCellsBySuccessOrFail(out successTargets, out failTargets, levelDiffuculty);

        if (pawn.botPawns.successCount > 0)
        {
            pawn.targetGrid = successTargets[Random.Range(0, successTargets.Count)];
            pawn.similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, pawn.targetGrid);
            
            pawn.targetGrid.vector.y = 1.5f;
            pawn.targetGrid.isEmpty = false;
        }
        else
        {
            pawn.targetGrid = failTargets[Random.Range(0, failTargets.Count)];
            pawn.similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, pawn.targetGrid);
            
            pawn.targetGrid.vector.y = 1.5f;
            pawn.targetGrid.isEmpty = false;
        }
    }
    void TakeGridCellsAndFindSimilarity(List<KeyValuePair<float, GridCell>> gridCellsSimilaritys)
    {
        foreach (GridCell cell in GridManager.Instance.gridDictionary.Values)
        {
            float similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, cell);
           
            gridCellsSimilaritys.Add(new KeyValuePair<float, GridCell>(similarity, cell));
        }
    }
    void ClassifyGridCellsBySuccessOrFail(out List<GridCell> successTargets, out List<GridCell> failTargets, int levelDifficulty)
    {
        List<KeyValuePair<float, GridCell>> gridCellsSimilaritys = new List<KeyValuePair<float, GridCell>>();
        TakeGridCellsAndFindSimilarity(gridCellsSimilaritys);
        successTargets = new List<GridCell>();
        failTargets = new List<GridCell>();
        foreach (var pair in gridCellsSimilaritys)
        {
            if (pair.Key < levelDifficulty && pair.Value.isEmpty == true)
            {
                failTargets.Add(pair.Value);
            }
            else if ((pair.Key >= levelDifficulty && pair.Key <= 95) && pair.Value.isEmpty == true)
            {
                successTargets.Add(pair.Value);
            }
        }
    }
    public void MovePawnsTowardsTarget(List<Pawn> pawns)
    {
        Sequence moveSequence = DOTween.Sequence();

        foreach (Pawn pawn in pawns)
        {
            if (pawn.pawnType == Pawn.PawnType.player) continue;

            moveSequence.Append(pawn.transform.DOJump(pawn.targetGrid.vector, jumpPower, numJumps, moveDuration)
            .OnStepComplete(() =>
            {
                colorManager.SetParticleColor(pawn.targetGrid);
                SoundManager.PlaySound(GameAssets.SoundType.hit);
            })
        );


            moveSequence.AppendInterval(delayBetweenPawns);
        }

        moveSequence.OnComplete(() =>
        {
            gameManager.FindSucsesPawns();
        });
    }
}
[System.Serializable]
public class BotPawns
{

    public int successCount;

    BotPawns(int successCount)
    {
        successCount = this.successCount;
    }
}