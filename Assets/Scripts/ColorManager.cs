using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    [SerializeField] GridManager gridManager;

    public List<Color> GenerateColorScale(int steps)
    {
        List<Color> colorList = new List<Color>();

        // Her renk çifti arasýnda geçiþ yapmak için adým sayýsýný hesapla
        int segmentSteps = steps / 3;
        float segmentFraction = 1f / segmentSteps;

        // Ýlk iki renk arasýnda geçiþ
        for (int i = 0; i < segmentSteps; i++)
        {
            float t = i * segmentFraction;
            Color color = Color.Lerp(color1, color2, t);
            colorList.Add(color);
        }

        // Ýkinci ve üçüncü renk arasýnda geçiþ
        for (int i = 0; i < segmentSteps; i++)
        {
            float t = i * segmentFraction;
            Color color = Color.Lerp(color2, color3, t);
            colorList.Add(color);
        }

        // Üçüncü ve dördüncü renk arasýnda geçiþ
        for (int i = 0; i < segmentSteps; i++)
        {
            float t = i * segmentFraction;
            Color color = Color.Lerp(color3, color4, t);
            colorList.Add(color);
        }

        // Son rengi ekle
        colorList.Add(color4);

        return colorList;
    }
    public Color CalculateCellColor(int x, int y)
    {
        // Normalleþtirilmiþ x ve y pozisyonlarý
        float normalizedX = (float)x / (gridManager.width - 1);
        float normalizedY = (float)y / (gridManager.height - 1);

        // Üst kenar için renk geçiþi (color1 -> color2)
        Color topColor = Color.Lerp(color1, color2, normalizedX);

        // Alt kenar için renk geçiþi (color3 -> color4)
        Color bottomColor = Color.Lerp(color3, color4, normalizedX);

        // Nihai renk (üst ve alt kenarlar arasýnda geçiþ)
        return Color.Lerp(bottomColor, topColor, normalizedY);
    }
}
