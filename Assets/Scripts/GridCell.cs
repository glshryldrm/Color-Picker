using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int x;
    public int z;
    public Vector3 vector;
    Color gridCellColor;
    private MeshRenderer meshRenderer;

    public Color GridCellColor { get => gridCellColor; set => gridCellColor = value; }

    public GridCell(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                    gridCellColor = color;
                }
                else
                {
                    mat.color = color;
                    gridCellColor = color;
                }
            }
        }
    }
    
}