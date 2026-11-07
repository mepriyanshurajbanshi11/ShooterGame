using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-center X hit marker.
/// Normal hit: white, normal scale.
/// Headshot or Kill: red, larger scale (1.5x).
/// Fades out in ~0.2 seconds.
/// </summary>
public class HitMarkerUI : MonoBehaviour
{
    private static HitMarkerUI instance;
    public static HitMarkerUI Instance => instance;

    [Header("Hit Marker Root")]
    public RectTransform hitMarkerRoot;
    public Image[] markerBars;

    [Header("Settings")]
    public float fadeDuration = 0.2f;
    public float normalScale = 1.0f;
    public float critScale = 1.5f;
    public Color bodyHitColor = new Color(1f, 1f, 1f, 0.95f);
    public Color critHitColor = new Color(1f, 0.15f, 0.15f, 1f);

    private float timer;
    private Color activeColor;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        EnsureHitMarkerUI();
        HideMarker();
    }

    void OnEnable()
    {
        CombatEvents.OnHit += HandleHit;
    }

    void OnDisable()
    {
        CombatEvents.OnHit -= HandleHit;
    }

    private void HandleHit(HitInfo hit)
    {
        ShowHitMarker(hit.isHeadshot || hit.isKill);
    }

    public void ShowHitMarker(bool isCritOrKill)
    {
        if (hitMarkerRoot == null) EnsureHitMarkerUI();
        if (hitMarkerRoot == null) return;

        hitMarkerRoot.gameObject.SetActive(true);
        hitMarkerRoot.localScale = Vector3.one * (isCritOrKill ? critScale : normalScale);

        activeColor = isCritOrKill ? critHitColor : bodyHitColor;
        SetBarsColor(activeColor);

        timer = fadeDuration;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            Color c = activeColor;
            c.a *= alpha;
            SetBarsColor(c);

            if (timer <= 0f)
            {
                HideMarker();
            }
        }
    }

    void HideMarker()
    {
        if (hitMarkerRoot != null)
        {
            hitMarkerRoot.gameObject.SetActive(false);
        }
    }

    void SetBarsColor(Color c)
    {
        if (markerBars == null) return;
        for (int i = 0; i < markerBars.Length; i++)
        {
            if (markerBars[i] != null) markerBars[i].color = c;
        }
    }

    /// <summary>
    /// Auto-generates the X hitmarker under Canvas if none was assigned.
    /// </summary>
    void EnsureHitMarkerUI()
    {
        if (hitMarkerRoot != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Check if already in canvas
        Transform existing = canvas.transform.Find("HitMarker");
        if (existing != null)
        {
            hitMarkerRoot = existing.GetComponent<RectTransform>();
            markerBars = existing.GetComponentsInChildren<Image>();
            return;
        }

        // Create root centered on screen
        GameObject rootGo = new GameObject("HitMarker", typeof(RectTransform));
        rootGo.transform.SetParent(canvas.transform, false);

        hitMarkerRoot = rootGo.GetComponent<RectTransform>();
        hitMarkerRoot.anchorMin = new Vector2(0.5f, 0.5f);
        hitMarkerRoot.anchorMax = new Vector2(0.5f, 0.5f);
        hitMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
        hitMarkerRoot.anchoredPosition = Vector2.zero;
        hitMarkerRoot.sizeDelta = new Vector2(60f, 60f);

        // 4 diagonal ticks for X pattern: 45°, 135°, 225°, 315°
        float[] angles = new float[] { 45f, 135f, 225f, 315f };
        float distance = 10f;
        markerBars = new Image[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject bar = new GameObject("Tick_" + i, typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(rootGo.transform, false);

            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(2.5f, 10f);

            float rad = angles[i] * Mathf.Deg2Rad;
            rt.anchoredPosition = new Vector2(Mathf.Sin(rad) * distance, Mathf.Cos(rad) * distance);
            rt.localEulerAngles = new Vector3(0f, 0f, -angles[i]);

            Image img = bar.GetComponent<Image>();
            img.color = bodyHitColor;
            img.raycastTarget = false;
            markerBars[i] = img;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        if (FindFirstObjectByType<HitMarkerUI>() == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.AddComponent<HitMarkerUI>();
            }
        }
    }
}
