using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scrtwpns.Mixbox;

public class ColorManager : MonoBehaviour
{
    [SerializeField] List<Color> colors;
    [SerializeField] GridManager gridManager;

    public Color CalculateCellColor(int q, int r)
    {
        if (colors == null || colors.Count < 2)
        {
            Debug.LogWarning("Renk listesi bos veya yeterli renge sahip degil.");
            return Color.white; // Default renk
        }
        Color finalColor;
        float normalizedQ = 0f;
        float normalizedR = 0f;
        if (gridManager.gridShape == GridManager.GridShapes.hexGrid)
        {
            int radius = gridManager.radius;

            normalizedQ = (float)(q + radius) / (2 * radius);
            normalizedR = (float)(r + radius) / (2 * radius);

        }
        else if (gridManager.gridShape == GridManager.GridShapes.rectangleGrid)
        {
            normalizedR = (float)q / (gridManager.width - 1);
            normalizedQ = (float)r / (gridManager.height - 1);
        }

        finalColor = CreateColor(colors, normalizedQ, normalizedR);
        return finalColor;
    }
    public void SetParticleColor(GridCell gridCell)
    {
        GameObject fx = Instantiate(GameAssets.Instance.particlePrefab, gridCell.vector, Quaternion.identity);
        var mainModule = fx.GetComponent<ParticleSystem>().main;
        //mainModule.startColor = gridCell.color;
        GameObject.Destroy(fx, mainModule.startLifetime.constant + 1f);
    }
    //public Color CreateColor(List<Color> colors, float qNorm, float rNorm)
    //{
    //    Color topColor = Color.black;
    //    Color bottomColor = Color.black;

    //    for (int i = 0; i < colors.Count / 2 - 1; i++)
    //    {
    //        topColor = Mixbox.Lerp(colors[i], colors[i + 1], rNorm);
    //    }

    //    for (int i = colors.Count - 1; i >= colors.Count / 2 + 1; i--)
    //    {
    //        bottomColor = Mixbox.Lerp(colors[i], colors[i - 1], rNorm);
    //    }


    //    Color finalColor = Mixbox.Lerp(topColor, bottomColor, qNorm);
    //    return finalColor;
    //}
    public Color CreateColor(List<Color> colors, float qNorm, float rNorm)
    {
        if (colors == null || colors.Count == 0)
        {
            return Color.black;
        }
        if (colors.Count == 1)
        {
            return colors[0];
        }
        if (colors.Count == 2)
        {
            return Mixbox.Lerp(colors[0], colors[1], qNorm);
        }

        Color topColor;
        Color bottomColor;

        if (colors.Count == 3)
        {
            topColor = Mixbox.Lerp(colors[0], colors[1], rNorm);
            bottomColor = colors[2];
        }
        else
        {
            int halfCount = colors.Count / 2;

            // Top color calculation
            int topIndex = Mathf.Clamp(Mathf.FloorToInt(rNorm * (halfCount - 1)), 0, halfCount - 1);
            float topLerp = rNorm * (halfCount - 1) - topIndex;
            topColor = Mixbox.Lerp(colors[topIndex], colors[topIndex + 1], topLerp);

            // Bottom color calculation
            int bottomIndex = Mathf.Clamp(Mathf.FloorToInt((1 - rNorm) * (halfCount - 1)), 0, halfCount - 1);
            float bottomLerp = (1 - rNorm) * (halfCount - 1) - bottomIndex;
            bottomColor = Mixbox.Lerp(colors[colors.Count - 1 - bottomIndex], colors[colors.Count - 2 - bottomIndex], bottomLerp);
        }

        // Final color calculation
        Color finalColor = Mixbox.Lerp(topColor, bottomColor, qNorm);
        return finalColor;
    }

}
