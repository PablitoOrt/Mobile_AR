using System.Collections;
using UnityEngine;

public class TargetContentAnimator : MonoBehaviour
{
    [SerializeField] private float appearDuration = 0.35f;
    [SerializeField] private AnimationCurve appearCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.7f, 1.15f), new Keyframe(1f, 1f));
    [SerializeField] private float spinSpeed = 45f;

    Vector3 originalScale;
    Coroutine appearRoutine;
    bool visible;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (visible && spinSpeed != 0f)
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }

    public void Show()
    {
        visible = true;
        if (appearRoutine != null)
            StopCoroutine(appearRoutine);
        appearRoutine = StartCoroutine(Appear());
    }

    public void Hide()
    {
        visible = false;
        if (appearRoutine != null)
            StopCoroutine(appearRoutine);
        transform.localScale = originalScale;
    }

    IEnumerator Appear()
    {
        for (float t = 0f; t < appearDuration; t += Time.deltaTime)
        {
            transform.localScale = originalScale * appearCurve.Evaluate(t / appearDuration);
            yield return null;
        }
        transform.localScale = originalScale;
        appearRoutine = null;
    }
}
