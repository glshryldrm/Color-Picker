using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GridManager gridManager;
    [SerializeField] ColorManager colorManager;
    [SerializeField] int x;
    [SerializeField] int z;
    GridCell targetGrid;
   
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
        gridManager.CalculateGrid();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    void FindGridsAndSetColor()
    {
        GridCell[] gridCells = FindObjectsOfType<GridCell>();
        colorList = colorManager.GenerateColorScale(colorManager.baseColor, gridManager.numberOfGrids);
        if(gridCells.Length == gridManager.numberOfGrids)
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
        targetGrid = gridManager.gridMatrix[x, z];
        Debug.Log(targetGrid.GridCellColor);
    }
}
