using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Screen dimmer overlay: creates a full-screen black Image and animates its alpha.
/// Use ScreenDimmer.Instance.Flash(duration, maxAlpha, frequency) to trigger.
/// </summary>
public class ScreenDimmer : MonoBehaviour
{
    public static ScreenDimmer Instance { get; private set; }

    private Image overlayImage;
    private Coroutine dimCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        EnsureOverlayExists();
    }

    private void EnsureOverlayExists()
    {
        if (overlayImage != null) return;

        // Create a Canvas (place at root so it doesn't inherit scale/rect from parent)
        GameObject canvasGO = new GameObject("ScreenDimmerCanvas");
        canvasGO.transform.SetParent(null);
        canvasGO.transform.localScale = Vector3.one;
        canvasGO.transform.localPosition = Vector3.zero;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // keep on top

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create overlay Image as child of the canvas
        GameObject imgGO = new GameObject("ScreenDimmerOverlay");
        imgGO.transform.SetParent(canvasGO.transform, false);
        overlayImage = imgGO.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0f);

        // Configure RectTransform to stretch full screen
        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Ensure the canvas is not scaled by any parent and set a high sorting order
        canvas.sortingOrder = 1000;
        canvasGO.layer = LayerMask.NameToLayer("UI");
        DontDestroyOnLoad(canvasGO);
    }

    /// <summary>
    /// Flash the screen dimmer for a duration, pulsing alpha up to maxAlpha synchronized with a frequency.
    /// </summary>
    public void Flash(float duration, float maxAlpha, float frequency = 2f)
    {
        if (overlayImage == null) EnsureOverlayExists();

        if (dimCoroutine != null)
            StopCoroutine(dimCoroutine);

        dimCoroutine = StartCoroutine(DoDim(duration, maxAlpha, frequency));
    }

    private IEnumerator DoDim(float duration, float maxAlpha, float frequency)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float cycleTime = elapsed * frequency;
            float sinValue = Mathf.Sin(cycleTime * Mathf.PI * 2f); // -1..1
            float factor = (sinValue + 1f) * 0.5f; // 0..1
            float alpha = Mathf.Lerp(0f, maxAlpha, factor);

            if (overlayImage != null)
            {
                Color c = overlayImage.color;
                c.a = alpha;
                overlayImage.color = c;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
        }

        dimCoroutine = null;
    }
}
