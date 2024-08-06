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

    }
    private void Awake()
    {
        gridManager.CreateHexGrid();
        FindGridsColor();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    private void Update()
    {
        CheckLevelComplate();
    }
    void PlacePawnsAroundHexGrid()
    {
        if (gridManager.gridDictionary == null || gridManager.gridDictionary.Count == 0) return;

        botPawns = new List<Pawn>();
        List<Vector3> hexBoundaryPositions;
        GetHexagonBoundaryPositions(numberOfPawns, gridManager.radius + 1, out hexBoundaryPositions);

        Vector3 playerPawnPosition = hexBoundaryPositions[0];
        playerPawn = Instantiate(GameAssets.Instance.pawnPrefab, playerPawnPosition, Quaternion.identity);
        Pawn playerPawnComponent = playerPawn.GetComponent<Pawn>();
        playerPawnComponent.pawnType = Pawn.PawnType.player;
        playerPawnComponent.position = playerPawnPosition;
        playerPawn.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        botPawns.Add(playerPawnComponent);

        int placedPawns = 0;
        float minDistanceFromPlayer = 1.0f;

        for (int i = 0; i < hexBoundaryPositions.Count; i++)
        {
            Vector3 position = hexBoundaryPositions[i];
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

        float hexRadius = radius;
        float angleStep = 360f / (count);

        for (int i = 0; i < count; i++)
        {

            float angle = ((i) * angleStep) + 270f;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3((Mathf.Cos(radians)) * hexRadius, 0.5f, (Mathf.Sin(radians)) * hexRadius);
            positions.Add(offset);

        }
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
        InputManager.touchCheck = false;
        playerPawn.GetComponent<Pawn>().similarity = CalculateDistancePercentage(targetGrid, selectedGrid);
        playerPawn.GetComponent<Pawn>().targetGrid = selectedGrid;
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
        foreach (GridCell pair in gridManager.gridDictionary.Values)
        {
            if (pair.isEmpty == true)
            {
                pair.GetComponentInChildren<Animator>().SetBool("isEmpty", true);
            }
        }
        foreach (Pawn pawn in botPawns)
        {
            if (pawn.targetGrid.isEmpty == true)
            {
                pawn.GetComponentInChildren<Animator>().SetBool("isSucsess", true);
            }
        }
        Invoke(nameof(DestroyFailPawns), 1.5f);
        Invoke(nameof(SetPawnsPosition), 2f);
    }
    void Fall2AnimStart()
    {
        foreach (GridCell pair in gridManager.gridDictionary.Values)
        {
            if (pair.isEmpty == true)
            {
                pair.GetComponentInChildren<Animator>().SetBool("isFall", true);
            }
        }
    }
    public void FindSucsesPawns()
    {
        foreach (Pawn pawn in botPawns)
        {
            if (pawn.similarity <= 50)
            {
                pawn.targetGrid.isEmpty = true;
            }
        }
        Invoke(nameof(FallAnimStart), 1f);
        Fall2AnimStart();
        Invoke(nameof(GetEmptyGrids), 3f);
    }
    void DestroyFailPawns()
    {
        List<Pawn> pawnsToDestroy = new List<Pawn>(botPawns);
        foreach (Pawn pawn in pawnsToDestroy)
        {
            if (pawn.targetGrid.isEmpty == true)
            {
                botPawns.Remove(pawn);
                Destroy(pawn.gameObject);
            }
        }
    }
    public void SetPawnsPosition()
    {
        foreach (Pawn pawn in botPawns)
        {
            pawn.GetComponent<Transform>().position = pawn.position;
        }
    }
    void GetEmptyGrids()
    {
        foreach (GridCell cell in gridManager.gridDictionary.Values)
        {
            cell.GetComponentInChildren<Animator>().SetBool("isFall", false);
            cell.GetComponentInChildren<Animator>().SetBool("isEmpty", false);
            cell.isEmpty = true;
        }
        InputManager.touchCheck = true;
    }
    void CheckLevelComplate()
    {
        if (botPawns.Count == 1)
        {
            foreach (Pawn pawn in botPawns)
            {
                if (pawn != null && pawn.pawnType == Pawn.PawnType.player)
                {
                    levelManager.LoadNextLevel();
                }
                else
                {
                    levelManager.ReloadLevel();
                }
            }
        }

    }
}
