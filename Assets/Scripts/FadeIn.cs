using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    private float animationTime;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = gameObject.transform.localScale;
        gameObject.transform.localScale = Vector3.zero;
    }

    void Start()
    {
        animationTime = Environment.instance.GetAnimationTime();
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        float time = 0.0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            float scale = Mathf.Sin(time / animationTime * (Mathf.PI / 2));
            gameObject.transform.localScale = originalScale * scale;
            yield return null;
        }
        gameObject.transform.localScale = originalScale;
    }
}