using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public Difficulty difficulty;
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
        // Listeyi rastgele karıştır
        Shuffle(gameManager.botPawns);

        // Piyonları zorluk seviyelerine göre ayır
        int totalPawns = gameManager.botPawns.Count;
        int easyCount = Mathf.CeilToInt(totalPawns * 0.33f); // Kolay zorluk piyonları
        int mediumCount = Mathf.CeilToInt(totalPawns * 0.33f); // Orta zorluk piyonları
        int hardCount = totalPawns - easyCount - mediumCount; // Zor zorluk piyonları

        easyPawns = gameManager.botPawns.GetRange(0, easyCount);
        mediumPawns = gameManager.botPawns.GetRange(easyCount, mediumCount);
        hardPawns = gameManager.botPawns.GetRange(easyCount + mediumCount, hardCount);

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
            if (!pawn.isMoving) continue;

            // Pawn'ı hedef pozisyona hareket ettir
            moveSequence.Append(pawn.transform.DOJump(pawn.targetGrid.vector, jumpPower, numJumps, moveDuration));

            // Her bir pawn hareketi arasında gecikme ekle
            moveSequence.AppendInterval(delayBetweenPawns);
        }
    }


    void PerformActionOnGrid(Pawn pawn)
    {
        foreach (GridCell cell in gridManager.gridDictionary.Values)
        {
            if (pawn.targetGrid == cell)
            {
                pawn.similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, cell);
                Debug.Log(pawn.similarity);
            }
        }
    }
    class GridSimilarity
    {
        public GridCell gridcell;
        public float similarity;
    }


    //public void DetermineBotTargets(List<Pawn> botPawns)
    //{
    //    foreach (var bot in botPawns)
    //    {
    //        Vector3 targetPosition = SelectBotTarget(bot.position);
    //        if (targetPosition != Vector3.zero)
    //        {
    //            TeleportBotToTarget(bot, targetPosition);
    //        }
    //    }
    //}
    //private Vector3 SelectBotTarget(Vector3 botPosition)
    //{
    //    Vector3 targetPosition = Vector3.zero;
    //    float bestSimilarity = difficulty == Difficulty.Hard ? float.MaxValue : float.MinValue;

    //    foreach (var cell in gridManager.gridDictionary)
    //    {
    //        float similarity = CalculateSimilarity(cell.Value.vector, botPosition);

    //        if (difficulty == Difficulty.Easy)
    //        {
    //            // For Easy AI, choose the farthest cell
    //            if (similarity < bestSimilarity)
    //            {
    //                bestSimilarity = similarity;
    //                targetPosition = cell.Value.vector;
    //            }
    //        }
    //        else if (difficulty == Difficulty.Medium)
    //        {
    //            // For Medium AI, choose a random cell within a range of similarity
    //            float range = 20f; // Adjust the range as needed
    //            if (similarity > bestSimilarity - range && similarity < bestSimilarity + range)
    //            {
    //                bestSimilarity = similarity;
    //                targetPosition = cell.Value.vector;
    //            }
    //        }
    //        else if (difficulty == Difficulty.Hard)
    //        {
    //            // For Hard AI, choose the closest cell
    //            if (similarity > bestSimilarity)
    //            {
    //                bestSimilarity = similarity;
    //                targetPosition = cell.Value.vector;
    //            }
    //        }
    //    }

    //    return targetPosition;
    //}
    //private float CalculateSimilarity(Vector3 targetPosition, Vector3 botPosition)
    //{
    //    float manhattanDistance = Mathf.Abs(botPosition.x - targetPosition.x) + Mathf.Abs(botPosition.z - targetPosition.z);
    //    float maxManhattanDistance;
    //    if (gridManager.gridShape == GridManager.GridShapes.hexGrid)
    //    {
    //        maxManhattanDistance = (gridManager.radius * 2);
    //    }
    //    else
    //    {
    //        maxManhattanDistance = gridManager.width + gridManager.height - 2;
    //    }

    //    float similarity = (maxManhattanDistance - manhattanDistance) / maxManhattanDistance * 100;
    //    similarity = Mathf.Ceil(similarity);
    //    //similarityText.text = "Similarity = %" + similarity;
    //    return similarity;
    //}

    //private void TeleportBotToTarget(Pawn bot, Vector3 targetPosition)
    //{
    //    // Implement the logic to teleport the bot to the target position
    //    bot.transform.position = targetPosition;
    //    bot.position = targetPosition;
    //    // Additional logic to update the bot's state or animation can be added here
    //}
}
