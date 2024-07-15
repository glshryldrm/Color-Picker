using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scrtwpns.Mixbox;

public class ColorManager : MonoBehaviour
{
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    [SerializeField] GridManager gridManager;

    public Color CalculateCellColor(int q, int r)
    {
        // Grid'in radius'u
        int radius = gridManager.radius;

        // Axial koordinatlarý normalleþtirme
        float normalizedQ = (float)(q + radius) / (2 * radius);
        float normalizedR = (float)(r + radius) / (2 * radius);

        // Üst kenar için renk geçiþi (topLeftColor -> topRightColor)
        Color topColor = Color.Lerp(color1, color2, normalizedR);

        // Alt kenar için renk geçiþi (bottomLeftColor -> bottomRightColor)
        Color bottomColor = Color.Lerp(color3, color4, normalizedR);

        // Nihai renk (üst ve alt kenarlar arasýnda geçiþ)
        //Color finalColor = Color.Lerp(bottomColor, topColor, normalizedR);
        Color finalColor = Mixbox.Lerp(topColor, bottomColor, normalizedQ);


        // Debug.Log ile ara deðerleri kontrol et
        Debug.Log($"q: {q}, r: {r}, normalizedQ: {normalizedQ}, normalizedR: {normalizedR}, topColor: {topColor}, bottomColor: {bottomColor}, finalColor: {finalColor}");

        return finalColor;
    }
    public void SetParticleColor(GridCell gridCell)
    {
        GameObject fx = Instantiate(GameAssets.Instance.particlePrefab, gridCell.vector, Quaternion.identity);
        var mainModule = fx.GetComponent<ParticleSystem>().main;
        mainModule.startColor = gridCell.gridCellColor;
        GameObject.Destroy(fx, mainModule.startLifetime.constant + 1f);
    }
}
