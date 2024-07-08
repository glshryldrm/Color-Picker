using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridManager : MonoBehaviour
{
    public int height;
    public int width;
    [HideInInspector] public int numberOfGrids;

    public GridCell[,] gridMatrix;
    //private void OnDrawGizmos()
    //{
    //    float offsetX = (width - 1) / 2f;
    //    float offsetZ = (height - 1) / 2f;

    //    for (int z = 0; z < height; z++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            Vector3 vector = new Vector3((transform.position.x + x) - offsetX, 0.5f, (transform.position.z + z) - offsetZ);
    //            Gizmos.DrawWireCube(vector, new Vector3(1, 1, 1));
    //            GUIStyle style = new GUIStyle();
    //            style.alignment = TextAnchor.MiddleCenter;
    //            style.normal.textColor = Color.black;
    //            UnityEditor.Handles.Label(vector, x.ToString() + " - " + z.ToString(), style);
    //            //burada kodu yorum satirina aldim cunku diger turlu apk alamiyordum
    //        }
    //    }
    //}
    public void SpawnGrid()
    {
        gridMatrix = new GridCell[height, width];
        numberOfGrids = height * width;
        float offsetX = (width - 1) / 2f;
        float offsetZ = (height - 1) / 2f;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 vector = new Vector3((transform.position.x + x) - offsetX, 0.5f, (transform.position.z + z) - offsetZ);
                GameObject g = Instantiate(GameAssets.Instance.gridPrefab, vector, Quaternion.identity);
                g.GetComponent<GridCell>().vector = vector;
                gridMatrix[x, z] = g.GetComponent<GridCell>();
               
            }
        }
    }
}
