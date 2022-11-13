using UnityEngine;
using System.Collections;

public class HintEffect : MonoBehaviour {
    private CanvasGroup canvasG;

    private void Start()
    {
        canvasG = GetComponent<CanvasGroup>();
        iTween.ValueTo(gameObject, iTween.Hash("from", 0.2f, "to", 0.65f, "time", 0.6f, "loopType", "pingpong", "onupdate", "OnUpdate"));
    }

    private void OnUpdate(float value)
    {
        canvasG.alpha = value;
    }
}
