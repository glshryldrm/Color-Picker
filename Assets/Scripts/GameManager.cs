using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] int x;
    [SerializeField] int z;
    [SerializeField] GameObject playerPawn;
    [HideInInspector] public GridCell targetGrid;
    [HideInInspector] public GridCell selectedGrid;



    List<Color> colorList = new List<Color>();

    private void Start()
    {
        //FindGridsAndSetColor();
        //FindTargetGridColor();
        Invoke("FindGridsAndSetColor", 0.01f);
        Invoke("FindTargetGridColor", 0.1f);

    }
    private void Awake()
    {
        gridManager.SpawnGrid();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void FindGridsAndSetColor()
    {
        GridCell[] gridCells = FindObjectsOfType<GridCell>();
        colorList = colorManager.GenerateColorScale(colorManager.baseColor, gridManager.numberOfGrids);
        if (gridCells.Length == gridManager.numberOfGrids)
        {
            for (int i = 0; i < gridCells.Length; i++)
            {
                gridCells[i].SetColor(colorList[i]);
            }
        }
        else
        {
            Debug.Log("gridcells ve numberofgrids sayisi esit degil.");
        }
    }
    void FindTargetGridColor()
    {
        if (x < gridManager.width && z < gridManager.height)
        {
            targetGrid = gridManager.gridMatrix[x, z];
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
        selectedGrid.vector.y = 1.5f;
        playerPawn.transform.position = selectedGrid.vector;
        Instantiate(GameAssets.Instance.particlePrefab, selectedGrid.vector, Quaternion.identity);
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
        float similarity = 1.0f - (distance / Mathf.Sqrt(3.0f));

        Debug.Log("% : " + similarity * 100);
    }
    void ShowTargetGrid()
    {

    }
}
