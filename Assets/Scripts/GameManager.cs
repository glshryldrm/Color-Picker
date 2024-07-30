using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    [SerializeField] LevelManager levelManager;
    public Hex targetHex;
    [HideInInspector] public GridCell targetGrid;
    [SerializeField] float levelTarget = 70;
    float similarity;
    public int numberOfPawns;
    public AIManager aIManager;
    public List<Pawn> botPawns;

    private void Start()
    {
        FindTargetGridColor();
        StartCoroutine(ShowTargetGrid());
        PlacePawnsAroundHexGrid();
        //StartCoroutine(LevelProgression());

    }
    private void Awake()
    {
        gridManager.CreateHexGrid();
        FindGridsColor();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void PlacePawnsAroundHexGrid()
    {
        CreatePlayerPawn();
        if (gridManager.gridDictionary == null || gridManager.gridDictionary.Count == 0) return;

        botPawns = new List<Pawn>();
        Vector3 playerPawnPosition = playerPawn.GetComponent<Pawn>().position;
        List<Vector3> hexBoundaryPositions;
        GetHexagonBoundaryPositions(numberOfPawns, gridManager.radius + 1, out hexBoundaryPositions);

        int placedPawns = 0;
        float minDistanceFromPlayer = 1.0f; // Minimum mesafe deðerini ayarlayýn

        foreach (Vector3 position in hexBoundaryPositions)
        {
            if (Vector3.Distance(position, playerPawnPosition) > minDistanceFromPlayer && placedPawns < numberOfPawns)
            {
                GameObject pawnAI = Instantiate(GameAssets.Instance.pawnPrefab, position, Quaternion.identity);
                Pawn pawn = pawnAI.GetComponent<Pawn>();
                if (pawn != null)
                {
                    pawn.pawnType = Pawn.PawnType.bot;
                    pawn.position = position;
                    botPawns.Add(pawn);
                    placedPawns++;
                }
            }
        }
    }
    void CreatePlayerPawn()
    {
        int radius = gridManager.radius;
        Vector3 position = new Vector3(0, 0.5f, -(radius + 1));
        playerPawn = Instantiate(GameAssets.Instance.pawnPrefab, position, Quaternion.identity);
        playerPawn.GetComponent<Pawn>().pawnType = Pawn.PawnType.player;
        playerPawn.GetComponent<Pawn>().position = position;
        playerPawn.GetComponent<MeshRenderer>().materials[0].color = Color.red;
    }
    void GetHexagonBoundaryPositions(int count, float radius, out List<Vector3> positions)
    {
        positions = new List<Vector3>();

        // Altýgenin yarýçapý
        float hexRadius = radius;

        // Altýgenin köþe açýlarý
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            // Açý hesapla ve radyan cinsinden dönüþtür
            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;

            // Altýgenin köþe pozisyonunu hesapla
            Vector3 offset = new Vector3(Mathf.Cos(radians) * hexRadius, 0, Mathf.Sin(radians) * hexRadius);
            positions.Add(new Vector3(0, 0.5f, 0) + offset);
        }
    }




    private bool IsOnGridBoundary(Hex hex)
    {
        int maxRadius = gridManager.radius;
        return Mathf.Abs(hex.q) == maxRadius || Mathf.Abs(hex.r) == maxRadius;
    }

    void FindGridsColor()
    {
        InputManager.touchCheck = false;

        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            pair.Value.GridCellColor = colorManager.CalculateCellColor(pair.Key.q, pair.Key.r);
        }
    }
    void SetGridsColor()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            pair.Value.SetColor();
            InputManager.touchCheck = true;
        }
    }
    void FindTargetGridColor()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            if (pair.Key.Equals(targetHex))
            {
                targetGrid = pair.Value;
                targetGrid.SetColor();
            }
        }

    }
    public void PlacePlayerPawn(GridCell selectedGrid)
    {
        selectedGrid.vector.y = 1.5f;
        playerPawn.GetComponent<Pawn>().similarity = CalculateDistancePercentage(targetGrid, selectedGrid);
        playerPawn.transform.DOJump(selectedGrid.vector, aIManager.jumpPower, aIManager.numJumps, aIManager.moveDuration).OnComplete(() =>
        {
            colorManager.SetParticleColor(selectedGrid);
            SoundManager.PlaySound(GameAssets.SoundType.bubble);
            InputManager.touchCheck = false;
            selectedGrid.isEmpty = false;
            PrintPlayerSimilarty();
            aIManager.DetermineBotTargets();
            aIManager.MovePawnsTowardsTarget(botPawns);
        });

    }
    public float CalculateDistancePercentage(GridCell targetGrid, GridCell selectedGrid)
    {
        float similarity;
        float manhattanDistance = Mathf.Abs(selectedGrid.vector.x - targetGrid.vector.x) + Mathf.Abs(selectedGrid.vector.z - targetGrid.vector.z);
        float maxManhattanDistance;
        if (gridManager.gridShape == GridManager.GridShapes.hexGrid)
        {
            maxManhattanDistance = (gridManager.radius * 2);
        }
        else
        {
            maxManhattanDistance = gridManager.width + gridManager.height - 2;
        }

        similarity = (maxManhattanDistance - manhattanDistance) / maxManhattanDistance * 100;
        similarity = Mathf.Ceil(similarity);
        return similarity;
    }
    void PrintPlayerSimilarty()
    {
        similarityText.text = "Similarity = %" + playerPawn.GetComponent<Pawn>().similarity.ToString();
    }
    private IEnumerator ShowTargetGrid()
    {
        targetGrid.SetColor();

        yield return new WaitForSeconds(3f);
        targetGrid.SetColor(Color.white);

        yield return new WaitForSeconds(1f);
        SetGridsColor();
        InputManager.touchCheck = true;
    }
    private void SituationBySimilarity()
    {
        if (similarity >= levelTarget)
        {
            levelManager.LoadNextLevel();
        }
        else
        {
            levelManager.ReloadLevel();
        }
    }
    void FallAnimStart()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            if (pair.Value.isEmpty == true)
            {
                pair.Value.GetComponentInChildren<Animator>().SetBool("isEmpty", true);
            }
        }
        foreach (Pawn pawn in botPawns)
        {
            if (pawn.isSuccess == false)
            {

            }
        }
    }
    void Fall2AnimStart()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            if (pair.Value.isEmpty == true)
            {
                pair.Value.GetComponentInChildren<Animator>().SetBool("isFall", true);
            }
        }
    }
    void FindSucsesPawns()
    {
        
    }
}
