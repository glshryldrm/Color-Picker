using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Grid/GridData", order = 1)]
public class GridData : ScriptableObject
{
    public List<Hex> gridHexs = new List<Hex>();
    public List<GridCell> gridCells = new List<GridCell>();
}