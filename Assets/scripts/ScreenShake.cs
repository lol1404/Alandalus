using UnityEngine;
using System.Collections;

/// <summary>
/// Simple screen shake controller. Attach to any persistent GameObject or let PlayerHealth find/create it.
/// Use ScreenShake.Instance.Shake(duration, magnitude, frequency) to trigger.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Trigger a camera shake. If another shake is running it will be stopped and replaced.
    /// </summary>
    public void Shake(float duration, float magnitude, float frequency = 20f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude, frequency));
    }

    private IEnumerator DoShake(float duration, float magnitude, float frequency)
    {
        Camera cam = Camera.main;
        if (cam == null)
            yield break;

        Transform camT = cam.transform;
        Vector3 originalPos = camT.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - Mathf.Clamp01(elapsed / duration);

            // frequency controls how fast direction changes; use Perlin or random for jitter
            Vector2 rand = Random.insideUnitCircle;
            Vector3 offset = new Vector3(rand.x, rand.y, 0f) * magnitude * damper;

            camT.localPosition = originalPos + offset;

            // step according to frequency
            float step = 1f / Mathf.Max(1f, frequency);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // restore
        camT.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
