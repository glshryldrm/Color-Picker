using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Scrtwpns.Mixbox;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] ColorManager colorManager;
    GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] GameObject panel;
    Hex targetHex;
    [HideInInspector] public GridCell targetGrid;
    public AIManager aIManager;
    public List<Pawn> botPawns;
    bool levelCompleteSoundPlayed = false;
    public float camMoveSpeed = 2f;
    public int stageCount = 1;
    static int currentStage = 1;

    private void Start()
    {
        GridManager.Instance.ListToDictionary();
        Initialize();
        FindGridsColor();
        PlacePawnsAroundHexGrid();
    }
    private void Awake()
    {
        FindStageCount();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void Initialize()
    {
        FindTargetGridColor();
        StartCoroutine(ShowTargetGrid());

    }
    void ReloadGrids()
    {
        foreach (GridCell cell in GridManager.Instance.gridDictionary.Values)
        {
            if (!cell.gameObject.activeSelf)
            {
                cell.GetComponentInChildren<Collider>().enabled = true;
                Fall2AnimStart();
                FindGridsColor();
                Initialize();
            }
        }
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
        playerPawn.GetComponentInChildren<MeshRenderer>().materials[0].color = Color.red;
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
        ChooseTargetGrid();
        foreach (KeyValuePair<Hex, GridCell> pair in GridManager.Instance.gridDictionary)
        {
            if (pair.Key.Equals(targetHex))
            {
                targetGrid = pair.Value;
                //targetGrid.SetColor();
            }
        }

    }
    void ChooseTargetGrid()
    {
        GridCell highestScoreCell = GridManager.Instance.stageTargetColors[currentStage - 1];
        AssignScoresToNeighborsWithCollider(highestScoreCell);
        targetHex = GridManager.Instance.gridDictionary.FirstOrDefault(x => x.Value == highestScoreCell).Key;
    }
    public void AssignScoresToNeighborsWithCollider(GridCell targetGrid)
    {
        foreach (var cell in GridManager.Instance.gridDictionary.Values)
        {
            cell.Score = 0;
        }
        // Hedef grid'in pozisyonunu al
        Vector3 targetPosition = targetGrid.transform.position;

        // Belirli bir yarýçap (örneðin 1.5f) içinde collider'larý ara
        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, 1f);

        foreach (var hitCollider in hitColliders)
        {
            GridCell neighborGrid = hitCollider.GetComponentInParent<GridCell>();

            if (neighborGrid != null && neighborGrid != targetGrid)
            {
                // Komþu gridlere puan ve renk ata
                neighborGrid.Score = 2; // Örneðin, komþular 2 puan alýr
                neighborGrid.SetColor(Color.yellow);
            }
        }

        // Hedef grid'e puan ve renk ata
        targetGrid.Score = 5;
        targetGrid.SetColor(Color.red);
        targetGrid.isTarget = true;
    }

    public void PlacePlayerPawn(GridCell selectedGrid)
    {
        InputManager.touchCheck = false;
        playerPawn.GetComponent<Pawn>().similarity = CalculateDistancePercentage(targetGrid, selectedGrid);

        playerPawn.GetComponent<Pawn>().targetGrid = selectedGrid;
        Vector3 pawnTG = selectedGrid.vector;
        pawnTG.y = 1.5f;
        playerPawn.transform.DOJump(pawnTG, aIManager.jumpPower, aIManager.numJumps, aIManager.moveDuration).OnComplete(() =>
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
    void PrintPlayerSimilarty()
    {
        similarityText.text = "Similarity = %" + playerPawn.GetComponent<Pawn>().similarity.ToString();
    }
    private IEnumerator ShowTargetGrid()
    {
        CheckStageState();
        SetPawnsPosition();
        Image pi = panel.GetComponent<Image>();
        pi.color = Color.white;
        panel.SetActive(true);

        yield return new WaitForSeconds(.5f);
        StartCoroutine(StartCountdown());
        targetGrid.color.a = 1f;
        pi.color = targetGrid.color;

        yield return new WaitForSeconds(3f);
        panel.SetActive(false);
        SetGridsColor();
    }
    IEnumerator StartCountdown()
    {
        int currentTime = 3;
        countdownText.gameObject.SetActive(true);
        while (currentTime >= 0)
        {
            // UI Text'i güncelle
            countdownText.text = currentTime.ToString();

            // 1 saniye bekle
            yield return new WaitForSeconds(1f);

            // Zamaný bir azalt
            currentTime--;
        }
        countdownText.gameObject.SetActive(false);
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
                pawn.transform.DOMoveY(-5, 1).SetDelay(Random.Range(0f, 1f)).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    pawn.gameObject.SetActive(false);
                    CheckLevelComplate();
                    //DestroyFailPawns();
                });
            }
        }
        Invoke(nameof(ReloadGrids), 3f);
    }
    void Fall2AnimStart()
    {
        foreach (GridCell pair in GridManager.Instance.gridDictionary.Values)
        {
            if (pair.isEmpty == true)
            {
                pair.transform.position = new Vector3(pair.transform.position.x, 0, pair.transform.position.z);
                pair.gameObject.SetActive(true);
                pair.transform.DOMove(pair.vector, 1.5f).SetDelay(Random.Range(0f, 1f)).SetEase(Ease.InOutExpo).OnComplete(() => pair.transform.position = pair.vector);

            }
        }
        ChangeGridsToEmpty();
        ScoreManager.Instance.CalculatePawnsTotalScore();
    }
    void ChangeGridsToEmpty()
    {
        foreach (var cell in GridManager.Instance.gridDictionary.Values)
        {
            cell.isEmpty = true;
        }
    }
    public void FindSucsesPawns()
    {
        foreach (Pawn pawn in botPawns)
        {
            if (pawn.pawnType == Pawn.PawnType.player)
            {
                if (pawn.similarity < aIManager.levelDiffuculty)
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
            if (pawn.gameObject.activeSelf == false)
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
                pawn.gameObject.SetActive(true);
                pawn.transform.position = pawn.position;
            }
        }
    }
    void FindStageCount()
    {
        stageCount = GridManager.Instance.stageTargetColors.Count;
    }
    void CheckStageState()
    {
        if (currentStage > stageCount)
            return;

        stageText.text = "Stage " + currentStage.ToString();

        currentStage += 1;

    }
    void CheckLevelComplate()
    {
        botPawns.RemoveAll(pawn => pawn == null);
        bool playerExists = botPawns.Exists(pawn => pawn.pawnType == Pawn.PawnType.player && pawn.gameObject.activeSelf == true);
        BotPawns bp = aIManager.pawns.OrderByDescending(cell => cell.successCount).FirstOrDefault();

        //if (!playerExists && !levelFailedSoundPlayed)
        //{
        //    SoundManager.PlaySound(GameAssets.SoundType.fail);
        //    LevelManager.Instance.ReloadLevel();
        //    levelFailedSoundPlayed = true;
        //    stageCount = 1;
        //    return;
        //}


        if (!levelCompleteSoundPlayed && currentStage > stageCount)
        {
            panel.SetActive(false);
            UIManager.Instance.ActivateScorePanel(true);
            StartCoroutine(LoadNextLevelAfterDelay());

        }
    }
    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        // Yeni seviyeye geç
        SoundManager.PlaySound(GameAssets.SoundType.success);
        LevelManager.Instance.LoadNextLevel();
        currentStage = 1;
        levelCompleteSoundPlayed = true;
        UIManager.Instance.ActivateScorePanel(false);
    }
}
