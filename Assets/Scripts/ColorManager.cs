using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public Color baseColor;
    [SerializeField] GridManager gridManager;

    public List<Color> GenerateColorScale(Color baseColor, int steps)
    {
        List<Color> colorList = new List<Color>();

        if (steps % 2 == 0)
        {
            // Çift sayý adým: Yarý sayýsýný kullan
            int halfSteps = steps / 2;

            // Add lighter shades
            for (int i = 0; i < halfSteps; i++)
            {
                float t = (float)i / halfSteps;
                Color color = Color.Lerp(Color.white, baseColor, t);
                colorList.Add(color);
            }

            // Add darker shades
            for (int i = 0; i < halfSteps; i++)
            {
                float t = (float)i / halfSteps;
                Color color = Color.Lerp(baseColor, Color.black, t);
                colorList.Add(color);
            }
        }
        else
        {
            // Tek sayý adým: Merkezi renk ekleyin
            int halfSteps = steps / 2;

            // Add lighter shades
            for (int i = 0; i < halfSteps; i++)
            {
                float t = (float)i / halfSteps;
                Color color = Color.Lerp(Color.white, baseColor, t);
                colorList.Add(color);
            }

            // Add darker shades
            for (int i = 0; i < halfSteps + 1; i++)
            {
                float t = (float)i / halfSteps;
                Color color = Color.Lerp(baseColor, Color.black, t);
                colorList.Add(color);
            }
        }

        return colorList;
    }

}
