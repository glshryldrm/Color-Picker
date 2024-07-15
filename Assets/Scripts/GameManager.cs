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

        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridMatrix)
        {

            //pair.Value.GridCellColor = colorList[x];
            pair.Value.GridCellColor = colorManager.CalculateCellColor(pair.Key.q, pair.Key.r);
            x++;
        }
    }
    void SetGridsColor()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridMatrix)
        {
            pair.Value.SetColor();
            InputManager.touchCheck = true;
        }
    }
    void FindTargetGridColor()
    {
        foreach (KeyValuePair<Hex, GridCell> pair in gridManager.gridMatrix)
        {
            if (pair.Key.Equals(hex))
            {
                targetGrid = pair.Value;
                targetGrid.SetColor();
                InputManager.touchCheck = true;
            }
        }

    }
    public void PlacePawn(GridCell selectedGrid)
    {
        this.selectedGrid = selectedGrid;
        selectedGrid.vector.y = 1f;
        colorManager.SetParticleColor(selectedGrid);
        playerPawn.transform.position = selectedGrid.vector;

        InputManager.touchCheck = false;

    }
    public void CalculateColorSimilarity(Color color1, Color color2)
    {
        // Renkler arasýndaki farklarý hesapla
        float rDiff = color1.r - color2.r;
        float gDiff = color1.g - color2.g;
        float bDiff = color1.b - color2.b;

        // Renk farkýnýn karelerini topla
        float distance = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

        // Mesafeyi benzerlik oranýna çevir (0 ile 1 arasýnda)
        similarity = (1.0f - (distance / Mathf.Sqrt(3.0f))) * 100;
        similarity = Mathf.Ceil(similarity);
        similarityText.text = "Similarity = %" + similarity;
        SituationBySimilarity();
    }
    private IEnumerator ShowTargetGrid()
    {
        targetGrid.SetColor();

        yield return new WaitForSeconds(1);
        targetGrid.SetColor(Color.white);

        yield return new WaitForSeconds(1);
        SetGridsColor();
    }
    private void SituationBySimilarity()
    {
        if (similarity >= 70)
        {
            levelManager.NextLevel();
        }
        else
        {
            levelManager.ReloadLevel();
        }
    }
}
