using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    private void Start()
    {
        MovePawnsTowardsTarget(easyPawns);
        MovePawnsTowardsTarget(mediumPawns);
        MovePawnsTowardsTarget(hardPawns);
    }


    public Difficulty difficulty;
    public GridManager gridManager;
    public GameManager gameManager;
    public float moveSpeed = 5f;

    public List<Pawn> easyPawns; // Kolay zorluk piyonlarý
    public List<Pawn> mediumPawns; // Orta zorluk piyonlarý
    public List<Pawn> hardPawns; // Zor zorluk piyonlarý

    public void DetermineBotTargets()
    {
        // Listeyi rastgele karýþtýr
        Shuffle(gameManager.botPawns);

        // Piyonlarý zorluk seviyelerine göre ayýr
        int totalPawns = gameManager.botPawns.Count;
        int easyCount = Mathf.CeilToInt(totalPawns * 0.33f); // Kolay zorluk piyonlarý
        int mediumCount = Mathf.CeilToInt(totalPawns * 0.33f); // Orta zorluk piyonlarý
        int hardCount = totalPawns - easyCount - mediumCount; // Zor zorluk piyonlarý

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
        List<Hex> allHexes = new List<Hex>(gridManager.gridDictionary.Keys);
        Hex targetHex = allHexes[Random.Range(0, allHexes.Count)];
        pawn.targetPosition = gridManager.HexToPosition(gameManager.targetHex);
        pawn.targetPosition.y = 1.5f;
        pawn.isMoving = true;
    }

    public void MovePawnsTowardsTarget(List<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            if (!pawn.isMoving) continue;

            // Hedef pozisyona doğru hareket et
            pawn.transform.position = Vector3.MoveTowards(pawn.transform.position, pawn.targetPosition, moveSpeed * Time.deltaTime);

            // Piyon hedefe ulaştığında yeni hedef belirle
            if (Vector3.Distance(pawn.transform.position, pawn.targetPosition) < 0.1f)
            {
                PerformActionOnGrid(pawn);
                SetNewTarget(pawn, pawn.difficulty);
            }
        }
    }



    void PerformActionOnGrid(Pawn pawn)
    {
        float similarity;
        bool success = false;
        foreach (GridCell cell in gridManager.gridDictionary.Values)
        {
            if (pawn.targetPosition == cell.vector)
            {
                similarity = gameManager.CalculateDistancePercentage(gameManager.targetGrid, cell);
                Debug.Log("Similarty =" + similarity);

                switch (pawn.difficulty)
                {
                    case Difficulty.Easy:
                        success = (similarity >= 50); // Kolay botlar %50 doðruluk payýna sahip
                        break;
                    case Difficulty.Medium:
                        success = similarity >= 70; // Orta botlar %70 doðruluk payýna sahip
                        break;
                    case Difficulty.Hard:
                        success = similarity >= 90; // Zor botlar %90 doðruluk payýna sahip
                        break;
                }
            }
        }

        if (success)
        {
            pawn.levelPassed++;
            if (pawn.levelPassed >= GetEliminationLevel(pawn.difficulty))
            {
                EliminatePawn(pawn);
            }
        }
        else
        {
            pawn.levelPassed = 0; // Baþarýsýz olursa geçilen seviye sýfýrlanýr
        }
    }
    int GetEliminationLevel(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 2;
            case Difficulty.Medium:
                return 5;
            case Difficulty.Hard:
                return 8;
            default:
                return 0;
        }
    }

    public void EliminatePawn(Pawn pawn)
    {
        pawn.gameObject.SetActive(false);
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
