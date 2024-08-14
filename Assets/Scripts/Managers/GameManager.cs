using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Scrtwpns.Mixbox;
public class GameManager : MonoBehaviour
{
    [SerializeField] ColorManager colorManager;
    GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    Hex targetHex;
    [HideInInspector] public GridCell targetGrid;
    public AIManager aIManager;
    public List<Pawn> botPawns;
    bool levelCompleteSoundPlayed = false;
    bool levelFailedSoundPlayed = false;
    public float camMoveSpeed = 2f;
    public float moveDistance = 15f;

    private void Start()
    {

        Initialize();
        PlacePawnsAroundHexGrid();
    }
    private void Awake()
    {
        GridManager.Instance.CreateHexGrid();
        FindGridsColor();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void ReloadGrids()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 forwardPosition = Camera.main.transform.position + new Vector3(0, 0, moveDistance);
        Camera.main.transform.DOMove(forwardPosition, camMoveSpeed).OnComplete(() =>
        {
            GridManager.Instance.CreateHexGrid(moveDistance);
            FindGridsColor();
            Initialize();
            Fall2AnimStart();

        });

    }
    void Initialize()
    {

        FindTargetGridColor();
        StartCoroutine(ShowTargetGrid());

    }
    void PlacePawnsAroundHexGrid()
    {
        if (GridManager.Instance.gridDictionary == null || GridManager.Instance.gridDictionary.Count == 0) return;

        botPawns = new List<Pawn>();

        List<Vector3> hexBoundaryPositions;
        GetHexagonBoundaryPositions(aIManager.pawns.Count + 1, GridManager.Instance.radius + 1, out hexBoundaryPositions);

        Vector3 playerPawnPosition = hexBoundaryPositions[0];
        playerPawn = Instantiate(GameAssets.Instance.pawnPrefab, playerPawnPosition, Quaternion.identity);
        Pawn playerPawnComponent = playerPawn.GetComponent<Pawn>();
        playerPawnComponent.pawnType = Pawn.PawnType.player;
        playerPawnComponent.position = playerPawnPosition;
        playerPawn.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        botPawns.Add(playerPawnComponent);

        int placedPawns = 0;

        for (int i = 1; i < hexBoundaryPositions.Count; i++)
        {
            Vector3 position = hexBoundaryPositions[i];
            if (placedPawns < aIManager.pawns.Count)
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
        int radius = GridManager.Instance.radius;
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

            Vector3 offset = new Vector3((Mathf.Cos(radians)) * hexRadius, 0f, (Mathf.Sin(radians)) * hexRadius);
            positions.Add(offset);

        }
    }

    void FindGridsColor()
    {
        InputManager.touchCheck = false;

        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
        {
            pair.Value.GridCellColor = colorManager.CalculateCellColor(pair.Key.q, pair.Key.r);
        }
    }
    void SetGridsColor()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
        {
            pair.Value.SetColor();

        }
        InputManager.touchCheck = true;
    }
    void FindTargetGridColor()
    {
        ChooseRandomTargetGrid();
        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
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
        int random = Random.Range(0, GridManager.Instance.gridDictionary.Count);
        targetHex = GridManager.Instance.gridDictionary.Keys.ElementAt(random);
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
        if (GridManager.Instance.gridShape == GridManager.GridShapes.hexGrid)
        {
            maxManhattanDistance = (GridManager.Instance.radius * 2);
        }
        else
        {
            maxManhattanDistance = GridManager.Instance.width + GridManager.Instance.height - 2;
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
        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
        {
            pair.Value.SetColor(targetGrid.color);
        }
        yield return new WaitForSeconds(3f);
        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
        {
            pair.Value.SetColor(Color.white);
        }
        yield return new WaitForSeconds(1f);
        SetGridsColor();
    }
    void FallAnimStart()
    {
        foreach (GridCell pair in GridManager.Instance.gridDictionary.Values)
        {
            if (pair != null && pair.isEmpty == true)
            {
                pair.transform.DOMoveY(-5, 1).SetDelay(Random.Range(0f, 1f)).SetEase(Ease.InExpo).OnComplete(() => pair.gameObject.SetActive(false));
            }
        }
        foreach (Pawn pawn in botPawns)
        {
            if (pawn != null && pawn.targetGrid.isEmpty == true)
            {
                pawn.transform.DOMoveY(-5, 1).SetDelay(Random.Range(0f, 1f)).SetEase(Ease.InExpo).OnComplete(() => DestroyFailPawns());
            }
        }
        CheckLevelComplate();
        Invoke(nameof(SetPawnsPosition), 2f);
        Invoke(nameof(ReloadGrids), 2f);

    }
    void Fall2AnimStart()
    {
        foreach (GridCell pair in GridManager.Instance.gridDictionary.Values)
        {
            if (pair.isEmpty == true)
            {
                pair.transform.position = new Vector3(pair.transform.position.x, Camera.main.transform.position.y + 5, pair.transform.position.z);
                pair.gameObject.SetActive(true);
                pair.transform.DOMove(pair.vector, 1.5f).SetDelay(Random.Range(0f, 1f)).SetEase(Ease.InOutExpo).OnComplete(() => pair.transform.position = pair.vector);

            }
        }

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
    }
    void DestroyFailPawns()
    {
        List<Pawn> pawnsToDestroy = new List<Pawn>(botPawns);
        foreach (Pawn pawn in pawnsToDestroy)
        {
            if (pawn.targetGrid.isEmpty == true && pawn != null && pawn.targetGrid != null)
            {
                aIManager.pawns.Remove(pawn.botPawns);
                botPawns.Remove(pawn);
                Destroy(pawn.gameObject);
            }
        }
    }
    public void SetPawnsPosition()
    {
        foreach (Pawn pawn in botPawns)
        {
            if (pawn != null)
            {
                pawn.position.z += moveDistance;
                pawn.transform.position = pawn.position;
            }
        }

    }
    void CheckLevelComplate()
    {
        bool playerExists = botPawns.Any(pawn => pawn.pawnType == Pawn.PawnType.player);

        if (!playerExists && !levelFailedSoundPlayed)
        {
            SoundManager.PlaySound(GameAssets.SoundType.fail);
            LevelManager.Instance.ReloadLevel();
            levelFailedSoundPlayed = true;
            return;
        }

        if (playerExists && botPawns.Count == 1 && !levelCompleteSoundPlayed)
        {
            SoundManager.PlaySound(GameAssets.SoundType.success);
            LevelManager.Instance.LoadNextLevel();
            levelCompleteSoundPlayed = true;
        }
    }
}
