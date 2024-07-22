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
        mainModule.startColor = gridCell.color;
        GameObject.Destroy(fx, mainModule.startLifetime.constant + 1f);
    }
    public Color CreateColor(List<Color> colors, float qNorm, float rNorm)
    {
        Color topColor = Color.black;
        Color bottomColor = Color.black;

        // Üst kenar için renk geçişi
        for (int i = 0; i < colors.Count - 1; i++)
        {
            topColor = Mixbox.Lerp(colors[i], colors[i + 1], rNorm);
        }

        // Alt kenar için renk geçişi
        for (int i = colors.Count - 1; i > 0; i--)
        {
            bottomColor = Mixbox.Lerp(colors[i], colors[i - 1], rNorm);
        }

        // Nihai renk (üst ve alt kenarlar arasında geçiş)
        Color finalColor = Mixbox.Lerp(topColor, bottomColor, qNorm);
        return finalColor;
    }
}
