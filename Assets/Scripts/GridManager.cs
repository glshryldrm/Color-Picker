using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int radius;
    public int width;
    public int height;
    [HideInInspector] public float hexRadius;
    [HideInInspector] public float hexWidth;
    [HideInInspector] public float hexHeight;
    public GameObject gizmosPrefab;
    public enum GridShapes
    {
        hexGrid,
        rectangleGrid
    }

    public GridShapes gridShape;
    public Dictionary<Hex, GridCell> gridDictionary;

    private void OnDrawGizmos()
    {
        MeshRenderer meshRenderer = gizmosPrefab.GetComponentInChildren<MeshRenderer>();
        hexWidth = meshRenderer.bounds.size.x;
        hexHeight = meshRenderer.bounds.size.z;

        if (gridShape == GridShapes.hexGrid)
        {
            hexRadius = hexWidth / Mathf.Sqrt(3);

            for (int q = -radius; q <= radius; q++)
            {
                for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
                {
                    Hex hexCoordinates = new Hex(q, r);
                    Vector3 position = HexToPosition(hexCoordinates);
                    position.y = 0.5f;
                    Gizmos.DrawWireMesh(gizmosPrefab.GetComponentInChildren<MeshFilter>().sharedMesh,
                        position,
                        this.transform.rotation,
                        gizmosPrefab.GetComponent<Transform>().localScale);
                        Gizmos.color = Color.white;
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.normal.textColor = Color.black;
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(position, q.ToString() + " - " + r.ToString(), style);
#endif
                }
            }
        }
        else if (gridShape == GridShapes.rectangleGrid)
        {
            // Grid'in ortalanması için gerekli ofsetler
            float offsetX = (width - 1) * hexWidth * 0.75f * 0.5f;
            float offsetZ = (height - 1) * hexHeight * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Hex hexCoordinates = new Hex(x, z);
                    Vector3 position = HexToPosition(hexCoordinates, hexWidth, hexHeight);
                    position.x -= offsetX; // X ekseninde ortala
                    position.z -= offsetZ; // Z ekseninde ortala
                    position.y = 0.5f;
                    Gizmos.DrawWireMesh(gizmosPrefab.GetComponentInChildren<MeshFilter>().sharedMesh,
                        position,
                        this.transform.rotation,
                        gizmosPrefab.GetComponent<Transform>().localScale);
                        Gizmos.color = Color.white;
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;
                    style.normal.textColor = Color.black;
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(position, x.ToString() + " - " + z.ToString(), style);
#endif
                }
            }
        }

    }

    public void CreateHexGrid()
    {
        MeshRenderer meshRenderer = GameAssets.Instance.gridPrefab.GetComponentInChildren<MeshRenderer>();
        hexWidth = meshRenderer.bounds.size.x;
        hexHeight = meshRenderer.bounds.size.z;

        if (gridShape == GridShapes.hexGrid)
        {
            if (GameAssets.Instance == null || GameAssets.Instance.gridPrefab == null) return;

            // Calculate radius
            hexRadius = hexWidth / Mathf.Sqrt(3);

            gridDictionary = new Dictionary<Hex, GridCell>();
            // Create the surrounding 
            for (int q = -radius; q <= radius; q++)
            {
                for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
                {
                    Hex hexCoordinates = new Hex(q, r);
                    Vector3 position = HexToPosition(hexCoordinates);
                    position.y = 0.5f;
                    GameObject hex = Instantiate(GameAssets.Instance.gridPrefab, position, Quaternion.identity);
                    hex.GetComponent<GridCell>().Initialize(hexCoordinates);
                    hex.GetComponent<GridCell>().vector = position;
                    gridDictionary[hexCoordinates] = hex.GetComponent<GridCell>();
                    hex.transform.parent = this.transform;
                }
            }
        }
        else if (gridShape == GridShapes.rectangleGrid)
        {
            // Grid'in ortalanması için gerekli ofsetler
            float offsetX = (width - 1) * hexWidth * 0.75f * 0.5f;
            float offsetZ = (height - 1) * hexHeight * 0.5f;

            gridDictionary = new Dictionary<Hex, GridCell>();
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Hex hexCoordinates = new Hex(x, z);
                    Vector3 position = HexToPosition(hexCoordinates, hexWidth, hexHeight);
                    position.x -= offsetX; // X ekseninde ortala
                    position.z -= offsetZ; // Z ekseninde ortala
                    position.y = 0.5f;
                    GameObject hex = Instantiate(GameAssets.Instance.gridPrefab, position, Quaternion.identity);
                    hex.GetComponent<GridCell>().Initialize(hexCoordinates);
                    hex.GetComponent<GridCell>().vector = position;
                    gridDictionary[hexCoordinates] = hex.GetComponent<GridCell>();
                    hex.transform.parent = this.transform;
                }
            }
        }
    }
    public Vector3 HexToPosition(Hex hex, float hexWidth, float hexHeight)
    {
        float x = hex.q * hexWidth * 0.75f;
        float z = hex.r * hexHeight + (hex.q % 2 == 0 ? 0 : hexHeight * 0.5f); // Satır bazında ofset uygular
        return new Vector3(x, 0, z);
    }
    public Vector3 HexToPosition(Hex hex)
    {
        float x = hexWidth * 0.75f * hex.q;
        float z = (Mathf.Sqrt(3.0f) / 2.0f * hex.q + Mathf.Sqrt(3.0f) * hex.r) * hexRadius * (hexHeight / hexWidth);
        return new Vector3(x, 0, z);
    }
}

[System.Serializable]
public class Hex
{
    public int q;
    public int r;

    public Hex(int q, int r)
    {
        this.q = q;
        this.r = r;
    }
    public bool Equals(Hex hex)
    {
        if (hex.q == this.q && hex.r == this.r)
        {
            return true;
        }
        else
            return false;
    }
}

