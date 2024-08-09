using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Scrtwpns.Mixbox;
public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] LevelManager levelManager;
    GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    Hex targetHex;
    [HideInInspector] public GridCell targetGrid;
    public AIManager aIManager;
    [HideInInspector] public List<Pawn> botPawns;
    bool levelCompleteSoundPlayed = false;
    bool levelFailedSoundPlayed = false;

    private void Start()
    {

        Initialize();
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
    void Initialize()
    {
        FindTargetGridColor();
        StartCoroutine(ShowTargetGrid());

    }
    void PlacePawnsAroundHexGrid()
    {
        if (gridManager.gridDictionary == null || gridManager.gridDictionary.Count == 0) return;

        botPawns = new List<Pawn>();

        List<Vector3> hexBoundaryPositions;
        GetHexagonBoundaryPositions(aIManager.pawns.Count + 1, gridManager.radius + 1, out hexBoundaryPositions);

        Vector3 playerPawnPosition = hexBoundaryPositions[0];
        playerPawn = Instantiate(GameAssets.Instance.pawnPrefab, playerPawnPosition, Quaternion.identity);
        Pawn playerPawnComponent = playerPawn.GetComponent<Pawn>();
        playerPawnComponent.pawnType = Pawn.PawnType.player;
        playerPawnComponent.position = playerPawnPosition;
        playerPawn.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        botPawns.Add(playerPawnComponent);

        int placedPawns = 0;
        float minDistanceFromPlayer = 1.0f;

        for (int i = 1; i < hexBoundaryPositions.Count; i++)
        {
            Vector3 position = hexBoundaryPositions[i];
            if (Vector3.Distance(position, playerPawnPosition) > minDistanceFromPlayer && placedPawns < aIManager.pawns.Count)
            {
                GameObject pawnAI = Instantiate(GameAssets.Instance.pawnPrefab, position, Quaternion.identity);
                Pawn pawn = pawnAI.GetComponent<Pawn>();
                if (pawn != null)
                {
                    pawn.pawnType = Pawn.PawnType.bot;
                    pawn.botPawns = aIManager.pawns[i - 1];
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

        }
        InputManager.touchCheck = true;
    }
    void FindTargetGridColor()
    {
        ChooseRandomTargetGrid();
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            if (pair.Key.Equals(targetHex))
            {
                targetGrid = pair.Value;
                //targetGrid.SetColor();
            }
        }

    }
    void ChooseRandomTargetGrid()
    {
        int random = Random.Range(0, gridManager.gridDictionary.Count);
        targetHex = gridManager.gridDictionary.Keys.ElementAt(random);
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
            SoundManager.PlaySound(GameAssets.SoundType.hit);
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
        similarity = Mathf.Clamp(similarity, 0f, 100f);

        similarity = Mathf.Ceil(similarity);
        return similarity;
    }
    public float CalculateColorSimilarty(GridCell targetGrid, GridCell selectedGrid)
    {
        // Hedef rengi latent uzaya dönüþtür
        MixboxLatent target = Mixbox.RGBToLatent(targetGrid.color);

        // Karþýlaþtýrýlan rengi latent uzaya dönüþtür ve hedefle arasýndaki farký hesapla
        MixboxLatent diff = target - Mixbox.RGBToLatent(selectedGrid.color);

        // Farkýn her bir bileþeni için mutlak deðerini al ve ortalamasýný hesapla
        float d = (Mathf.Abs(diff.c0) + Mathf.Abs(diff.c1) + Mathf.Abs(diff.c2) + Mathf.Abs(diff.c3)) / 4f;

        // Benzerlik yüzdesini hesapla (100 - ortalama fark)
        float similarPercent = 100 - (d * 100f);

        // Eðer bu benzerlik yüzdesi mevcut maksimumdan büyükse, maxRate'i güncelle
        return similarPercent;

    }
    void PrintPlayerSimilarty()
    {
        similarityText.text = "Similarity = %" + playerPawn.GetComponent<Pawn>().similarity.ToString();
    }
    private IEnumerator ShowTargetGrid()
    {
        yield return new WaitForSeconds(2f);
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            pair.Value.SetColor(targetGrid.color);
        }
        yield return new WaitForSeconds(3f);
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {
            pair.Value.SetColor(Color.white);
        }
        yield return new WaitForSeconds(1f);
        SetGridsColor();
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
        Invoke(nameof(DestroyFailPawns), 1f);
        Invoke(nameof(SetPawnsPosition), 2f);
        Fall2AnimStart();

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
        Initialize();
    }
    public void FindSucsesPawns()
    {
        foreach (Pawn pawn in botPawns)
        {
            if (pawn.pawnType == Pawn.PawnType.player)
            {
                if (playerPawn.GetComponent<Pawn>().similarity < aIManager.levelDiffuculty)
                {
                    pawn.targetGrid.isEmpty = true;
                }
            }
            else
            {
                if (pawn.botPawns.successCount == 0)
                {
                    pawn.targetGrid.isEmpty = true;
                }
                else
                {
                    pawn.botPawns.successCount -= 1;
                }
            }

        }
        FallAnimStart();
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
            cell.GetComponentInChildren<Animator>().SetBool("isRepeat", true);
            cell.isEmpty = true;
        }
        InputManager.touchCheck = true;
    }
    void CheckLevelComplate()
    {
        bool playerExists = false;

        foreach (Pawn pawn in botPawns)
        {
            if (pawn != null && pawn.pawnType == Pawn.PawnType.player)
            {
                playerExists = true;
                if (botPawns.Count == 1 && !levelCompleteSoundPlayed)
                {
                    SoundManager.PlaySound(GameAssets.SoundType.success);
                    levelManager.LoadNextLevel();
                    levelCompleteSoundPlayed = true;
                }
            }
            if (!playerExists && !levelFailedSoundPlayed)
            {
                SoundManager.PlaySound(GameAssets.SoundType.fail);
                levelManager.ReloadLevel();
                levelFailedSoundPlayed = true;
            }
        }
    }
}
