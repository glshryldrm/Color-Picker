using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    [SerializeField] LevelManager levelManager;
    public Hex targetHex;
    [HideInInspector] public GridCell targetGrid;
    [HideInInspector] public GridCell selectedGrid;
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
        if (gridManager.gridDictionary == null || gridManager.gridDictionary.Count == 0) return;
        botPawns = new List<Pawn>();
        Vector3 playerPawnPosition = playerPawn.transform.position;
        List<Vector3> hexBoundaryPositions;
        GetHexagonBoundaryPositions(numberOfPawns, gridManager.radius + 1, out hexBoundaryPositions);
        int placedPawns = 0;

        foreach (Vector3 position in hexBoundaryPositions)
        {
            if (Vector3.Distance(position, playerPawnPosition) > 0.1f && placedPawns < numberOfPawns) // Player pawn'ýn bulunduðu konumu atla
            {
                GameObject pawnAI = Instantiate(GameAssets.Instance.pawnPrefab, position, Quaternion.identity);
                Pawn pawn = pawnAI.GetComponent<Pawn>();
                if (pawn != null)
                {
                    pawn.position = position;
                    botPawns.Add(pawn);
                    placedPawns++;
                }
            }
        }
        aIManager.DetermineBotTargets();
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
    public void PlacePawn(GridCell selectedGrid)
    {
        this.selectedGrid = selectedGrid;
        selectedGrid.vector.y = 1.5f;
        colorManager.SetParticleColor(selectedGrid);
        playerPawn.transform.position = selectedGrid.vector;
        SoundManager.PlaySound(GameAssets.SoundType.bubble);
        InputManager.touchCheck = false;
        selectedGrid.isEmpty = false;
        aIManager.MovePawnsTowardsTarget(aIManager.easyPawns);
        aIManager.MovePawnsTowardsTarget(aIManager.mediumPawns);
        aIManager.MovePawnsTowardsTarget(aIManager.hardPawns);
    }
    public void CalculateColorSimilarity(Color color1, Color color2)
    {
        float rDiff = color1.r - color2.r;
        float gDiff = color1.g - color2.g;
        float bDiff = color1.b - color2.b;

        float distance = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

        similarity = (1.0f - (distance / Mathf.Sqrt(3.0f))) * 100;
        similarity = Mathf.Ceil(similarity);
        similarityText.text = "Similarity = %" + similarity;
        Invoke(nameof(SituationBySimilarity), 1f);
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
        this.similarity = similarity;
        similarityText.text = "Similarity = %" + this.similarity;
        FallAnimStart();
        Invoke(nameof(Fall2AnimStart), 1f);
        Invoke(nameof(SituationBySimilarity), 5f);
        return similarity;
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
}
