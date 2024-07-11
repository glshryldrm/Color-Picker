using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public float hexRadius;
    [HideInInspector] public int numberOfGrids;
    public GameObject gizmosPrefab;

    public GridCell[,] gridMatrix;

    private void OnDrawGizmos()
    {
        MeshRenderer meshRenderer = gizmosPrefab.GetComponentInChildren<MeshRenderer>();
        float hexWidth = meshRenderer.bounds.size.x;
        float hexHeight = meshRenderer.bounds.size.z;

        // Radius hesapla
        hexRadius = hexWidth / Mathf.Sqrt(3);

        // Grid'in ortalanması için gerekli ofsetler
        float offsetX = (width - 1) * hexWidth * 0.75f * 0.5f;
        float offsetZ = (height - 1) * hexHeight * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hex hexCoordinates = new Hex(x, y);
                Vector3 position = HexToPosition(hexCoordinates, hexWidth, hexHeight);
                position.x -= offsetX; // X ekseninde ortala
                position.z -= offsetZ; // Z ekseninde ortala
                position.y = 1f;
                Gizmos.DrawWireMesh(gizmosPrefab.GetComponentInChildren<MeshFilter>().sharedMesh,
                    position,
                    this.transform.rotation,
                    gizmosPrefab.GetComponent<Transform>().localScale);
                Gizmos.color = Color.white;
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.black;
                UnityEditor.Handles.Label(position, x.ToString() + " - " + y.ToString(), style);
                //burada kodu yorum satirina aldim cunku diger turlu apk alamiyordum
            }
        }
    }
    public void CreateHexGrid()
    {
        MeshRenderer meshRenderer = GameAssets.Instance.gridPrefab.GetComponentInChildren<MeshRenderer>();
        float hexWidth = meshRenderer.bounds.size.x;
        float hexHeight = meshRenderer.bounds.size.z;

        // Radius hesapla
        hexRadius = hexWidth / Mathf.Sqrt(3);

        gridMatrix = new GridCell[width, height];
        numberOfGrids = width * height;

        // Grid'in ortalanması için gerekli ofsetler
        float offsetX = (width - 1) * hexWidth * 0.75f * 0.5f;
        float offsetZ = (height - 1) * hexHeight * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hex hexCoordinates = new Hex(x, y);
                Vector3 position = HexToPosition(hexCoordinates, hexWidth, hexHeight);
                position.x -= offsetX; // X ekseninde ortala
                position.z -= offsetZ; // Z ekseninde ortala
                position.y = 1f;
                GameObject hex = Instantiate(GameAssets.Instance.gridPrefab, position, Quaternion.identity);
                hex.GetComponent<GridCell>().Initialize(hexCoordinates);
                hex.GetComponent<GridCell>().vector = position;
                gridMatrix[x, y] = hex.GetComponent<GridCell>();
                hex.transform.parent = this.transform;
            }
        }
    }

    Vector3 HexToPosition(Hex hex, float hexWidth, float hexHeight)
    {
        float x = hex.q * hexWidth * 0.75f;
        float z = hex.r * hexHeight + (hex.q % 2 == 0 ? 0 : hexHeight * 0.5f); // Satır bazında ofset uygular
        return new Vector3(x, 0, z);
    }
}

public class Hex
{
    public int q;
    public int r;

    public Hex(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public Cube ToCube()
    {
        int s = -q - r;
        return new Cube(q, r, s);
    }
}

public class Cube
{
    public int q;
    public int r;
    public int s;

    public Cube(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (q + r + s != 0)
        {
            throw new System.ArgumentException("Cube coordinates must satisfy q + r + s = 0");
        }
    }

    public Hex ToAxial()
    {
        return new Hex(q, r);
    }
}
