using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 baseLocalPosition;
    Coroutine shakeRoutine;

    void Awake() => baseLocalPosition = transform.localPosition;

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    IEnumerator ShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float damper = 1f - elapsed / duration;
            transform.localPosition = baseLocalPosition + (Vector3)(Random.insideUnitCircle * strength * damper);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = baseLocalPosition;
        shakeRoutine = null;
    }
}