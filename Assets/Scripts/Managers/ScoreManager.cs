using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] List<GridCell> scoredGrids = new List<GridCell>();
    private void Awake()
    {
        Instance = this;
    }
    public void FindScoredGrids()
    {
        foreach (var cell in GridManager.Instance.gridDictionary.Values)
        {
            if (cell.Score != 0)
            {
                scoredGrids.Add(cell);
            }
        }
    }
}
