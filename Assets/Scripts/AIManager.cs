using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIManager : MonoBehaviour
{
    public enum Difficulty
    {
        Default,
        Easy,
        Medium,
        Hard
    }
    public Difficulty difficulty = Difficulty.Default;
    public GridManager gridManager;
    public GameManager gameManager;
    public float moveSpeed = 5f;
    public float moveDuration = 1f; // Pawn hareket süresi
    public float delayBetweenPawns = 0.2f; // Pawn hareketleri arasındaki gecikme
    public float jumpPower = 1f; // Pawn'ın yukarı zıplama gücü
    public int numJumps = 1; // Pawn'ın zıplama sayısı

    public List<Pawn> easyPawns; // Kolay zorluk piyonlarý
    public List<Pawn> mediumPawns; // Orta zorluk piyonlarý
    public List<Pawn> hardPawns; // Zor zorluk piyonlarý
    Dictionary<float, GridCell> gridCellsSimilaritys = new Dictionary<float, GridCell>();

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
        // Listeyi rastgele karıştır
        int totalPawns, easyCount, mediumCount, hardCount;
        Shuffle(botPawns);
        if (botPawns.Count < 3)
        {
            easyCount = botPawns.Count;
            easyPawns = botPawns.GetRange(0, easyCount);

        }
        else
        {
            totalPawns = botPawns.Count;
            easyCount = Mathf.CeilToInt(totalPawns * 0.33f); // Kolay zorluk piyonları
            mediumCount = Mathf.CeilToInt(totalPawns * 0.33f); // Orta zorluk piyonları
            hardCount = totalPawns - easyCount - mediumCount; // Zor zorluk piyonları

            easyPawns = botPawns.GetRange(0, easyCount);
            mediumPawns = botPawns.GetRange(easyCount, mediumCount);
            hardPawns = botPawns.GetRange(easyCount + mediumCount, hardCount);
        }
        foreach (var pawn in easyPawns)
        {
            pawn.difficulty = Difficulty.Easy;
            SetNewTarget(pawn, Difficulty.Easy);
        }

        foreach (var pawn in mediumPawns)
        {
            pawn.difficulty = Difficulty.Medium;
            SetNewTarget(pawn, Difficulty.Medium);
        }

        foreach (var pawn in hardPawns)
        {
            pawn.difficulty = Difficulty.Hard;
            SetNewTarget(pawn, Difficulty.Hard);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    void SetNewTarget(Pawn pawn, Difficulty difficulty)
    {
        List<GridCell> easyTargets;
        List<GridCell> mediumTargets;
        List<GridCell> hardTargets;
        ClassifyGridCellsByDifficulty(out easyTargets, out mediumTargets, out hardTargets);

        if (difficulty == Difficulty.Easy)
        {
            pawn.targetGrid = easyTargets[Random.Range(0, easyTargets.Count)];
        }
        else if (difficulty == Difficulty.Medium)
        {
            pawn.targetGrid = mediumTargets[Random.Range(0, mediumTargets.Count)];
        }
        else if (difficulty == Difficulty.Hard)
        {
            pawn.targetGrid = hardTargets[Random.Range(0, hardTargets.Count)];
        }
        pawn.similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, pawn.targetGrid);
        pawn.targetGrid.vector.y = 1.5f;
        pawn.isMoving = true;
        pawn.targetGrid.isEmpty = false;
    }

    void TakeGridCellsAndFindSimilarity(List<KeyValuePair<float, GridCell>> gridCellsSimilaritys)
    {
        foreach (GridCell cell in gridManager.gridDictionary.Values)
        {
            float similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, cell);
            gridCellsSimilaritys.Add(new KeyValuePair<float, GridCell>(similarity, cell));
        }
    }

    void ClassifyGridCellsByDifficulty(out List<GridCell> easyTargets, out List<GridCell> mediumTargets, out List<GridCell> hardTargets)
    {
        List<KeyValuePair<float, GridCell>> gridCellsSimilaritys = new List<KeyValuePair<float, GridCell>>();
        TakeGridCellsAndFindSimilarity(gridCellsSimilaritys);

        easyTargets = new List<GridCell>();
        mediumTargets = new List<GridCell>();
        hardTargets = new List<GridCell>();

        foreach (var pair in gridCellsSimilaritys)
        {
            if (pair.Key < 60 && pair.Value.isEmpty)
            {
                easyTargets.Add(pair.Value);
            }
            else if (pair.Key >= 60 && pair.Key < 80 && pair.Value.isEmpty)
            {
                mediumTargets.Add(pair.Value);
            }
            else if (pair.Key >= 80 && pair.Key <= 95 && pair.Value.isEmpty)
            {
                hardTargets.Add(pair.Value);
            }
        }
    }

    public void MovePawnsTowardsTarget(List<Pawn> pawns)
    {
        Sequence moveSequence = DOTween.Sequence();

        foreach (var pawn in pawns)
        {
            if (!pawn.isMoving || pawn.pawnType == Pawn.PawnType.player) continue;

            moveSequence.Append(pawn.transform.DOJump(pawn.targetGrid.vector, jumpPower, numJumps, moveDuration));

            moveSequence.AppendInterval(delayBetweenPawns);
        }

        moveSequence.OnComplete(() =>
        {
            gameManager.FindSucsesPawns();
        });
    }
}
