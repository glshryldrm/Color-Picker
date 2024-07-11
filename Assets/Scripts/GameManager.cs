using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] int x;
    [SerializeField] int z;
    [SerializeField] GameObject playerPawn;
    [SerializeField] TextMeshProUGUI similarityText;
    [SerializeField] LevelManager levelManager;
    [HideInInspector] public GridCell targetGrid;
    [HideInInspector] public GridCell selectedGrid;
    
    float similarity;
    


    List<Color> colorList = new List<Color>();

    private void Start()
    {
        //FindTargetGridColor();
        //StartCoroutine(ShowTargetGrid());

    }
    private void Awake()
    {
        //gridManager.SpawnGrid();
        gridManager.CreateHexGrid();
        //FindGridsColor();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void FindGridsColor()
    {
        InputManager.touchCheck = false;
        //colorList = colorManager.GenerateColorScale(gridManager.numberOfGrids);
        int x = 0;
        //if (colorList.Count == gridManager.numberOfGrids && x <= colorList.Count)
        {
            for (int i = 0; i < gridManager.width; i++)
            {
                for (int j = 0; j < gridManager.height; j++)
                {
                    //gridManager.gridMatrix[i, j].GridCellColor = colorManager.CalculateCellColor(i, j);
                    //gridManager.gridMatrix[i, j].GridCellColor = colorList[x];
                    x++;
                }
            }
        }
    }
    void SetGridsColor()
    {
        for (int i = 0; i < gridManager.width; i++)
        {
            for (int j = 0; j < gridManager.height; j++)
            {
                //gridManager.gridMatrix[i, j].SetColor();
            }
        }
        InputManager.touchCheck = true;
    }
    void FindTargetGridColor()
    {
        if (x < gridManager.width && z < gridManager.height)
        {
            //targetGrid = gridManager.gridMatrix[x, z];
            Debug.Log(targetGrid.GridCellColor);
        }
        else
        {
            Debug.Log("targetGrid grid uzunlugunun disinda!!");
            InputManager.touchCheck = false;
        }

    }
    public void PlacePawn(GridCell selectedGrid)
    {
        this.selectedGrid = selectedGrid;
        selectedGrid.vector.y = 2f;
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
