using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] Hex hex;
    [SerializeField] GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    [SerializeField] LevelManager levelManager;
    [HideInInspector] public GridCell targetGrid;
    [HideInInspector] public GridCell selectedGrid;

    float similarity;

    private void Start()
    {
        FindTargetGridColor();
        StartCoroutine(ShowTargetGrid());

    }
    private void Awake()
    {
        gridManager.CreateHexGrid();
        FindGridsColor();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void FindGridsColor()
    {
        InputManager.touchCheck = false;
        int x = 0;

        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridDictionary)
        {

            //pair.Value.GridCellColor = colorList[x];
            pair.Value.GridCellColor = colorManager.CalculateCellColor(pair.Key.q, pair.Key.r);
            x++;
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
            if (pair.Key.Equals(hex))
            {
                targetGrid = pair.Value;
                targetGrid.SetColor();
            }
        }

    }
    public void PlacePawn(GridCell selectedGrid)
    {
        this.selectedGrid = selectedGrid;
        selectedGrid.vector.y = 1f;
        colorManager.SetParticleColor(selectedGrid);
        playerPawn.transform.position = selectedGrid.vector;
        SoundManager.PlaySound(GameAssets.SoundType.bubble);
        InputManager.touchCheck = false;
        selectedGrid.isEmpty = false;
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
    public void CalculateDistancePercentage(GridCell targetGrid, GridCell selectedGrid)
    {
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
        similarityText.text = "Similarity = %" + similarity;
        Invoke(nameof(SituationBySimilarity), 1f);

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
        if (similarity >= 70)
        {
            levelManager.LoadNextLevel();
        }
        else
        {
            levelManager.ReloadLevel();
        }
    }
}
