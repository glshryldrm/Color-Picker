using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int x;
    public int z;
    public Vector3 vector;
    public Color gridCellColor;
    private MeshRenderer meshRenderer;
    public Hex hexCoordinates;

    public void Initialize(Hex coordinates)
    {
        hexCoordinates = coordinates;
        transform.name = $"Hex ({coordinates.q}, {coordinates.r})";
    }
    public Color GridCellColor { get => gridCellColor; set => gridCellColor = value; }

    public void SetColor()
    {
        if ((meshRenderer = GetComponentInChildren<MeshRenderer>()) != null)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", gridCellColor);
                }
                else
                {
                    mat.color = gridCellColor;
                }
            }
        }
        else
            Debug.Log("meshrenderer bulunamadý");
    }
    public void SetColor(Color color)
    {
        if ((meshRenderer = GetComponent<MeshRenderer>()) != null)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }
                else
                {
                    mat.color = color;
                }
            }
        }
        else
            Debug.Log("meshrenderer bulunamadý");
    }
}