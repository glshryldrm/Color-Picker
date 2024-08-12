using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Analytics;
public class SceneTransition : MonoBehaviour
{
    [SerializeField] Vector2 referanceResolution;
    [SerializeField] Color color;
    [SerializeField] float delayTime;
    GameObject transitionObj;
    Image transitionImage;

    void Awake()
    {
        transitionObj = new GameObject("TransitionCanvas");
        var c = transitionObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        var cs = transitionObj.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = referanceResolution;

        transitionImage = new GameObject("TransitionImage").AddComponent<Image>();
        transitionImage.transform.SetParent(transitionObj.transform);
        transitionImage.color = color;
        transitionImage.rectTransform.anchorMin = new Vector2(0, 0);
        transitionImage.rectTransform.anchorMax = new Vector2(1, 1);
        transitionImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        transitionImage.rectTransform.sizeDelta = Vector2.zero;
        transitionImage.rectTransform.anchoredPosition = Vector2.zero;

        Color c_ = color;
        c_.a = 0;

        transitionImage.DOColor(c_, delayTime).OnComplete(() => transitionImage.gameObject.SetActive(false));

    }

    public void LoadScene(string sceneName)
    {
        Color c = color;
        c.a = 0;
        transitionImage.color = c;

        transitionImage.gameObject.SetActive(true);

        transitionImage.DOColor(color, delayTime).OnComplete(() => SceneManager.LoadScene(sceneName));
    }
}
