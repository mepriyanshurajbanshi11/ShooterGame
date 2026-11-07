using UnityEngine;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    [Header("Core References")]
    public Gun gun;
    public PlayerHealth health;
    public PlayerController playerController;
    public PlayerInputRouter inputRouter;

    [Header("Text Displays")]
    public Text ammoText;
    public Text healthText;
    public Text deathText;

    [Header("Health Bar (Bottom-Left)")]
    public Image healthBarFill;
    public Image healthBarBackground;

    [Header("Crosshair (Center)")]
    public RectTransform crosshairRoot;
    public RectTransform crosshairTop;
    public RectTransform crosshairBottom;
    public RectTransform crosshairLeft;
    public RectTransform crosshairRight;

    [Header("Crosshair Tuning")]
    public float baseSpread = 14f;
    public float moveSpread = 26f;
    public float sprintSpread = 38f;
    public float crouchSpread = 8f;
    public float proneSpread = 4f;
    public float spreadSpeed = 12f;

    private float currentSpread;

    void Awake()
    {
        // Cache references to player systems
        if (health == null) health = FindFirstObjectByType<PlayerHealth>();
        if (gun == null) gun = FindFirstObjectByType<Gun>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (inputRouter == null) inputRouter = FindFirstObjectByType<PlayerInputRouter>();

        currentSpread = baseSpread;
    }

    void Start()
    {
        // Auto-build UI elements if not wired in the inspector
        EnsureHealthBar();
        EnsureCrosshair();
    }

    void Update()
    {
        UpdateAmmo();
        UpdateHealth();
        UpdateCrosshair();
    }

    void UpdateAmmo()
    {
        if (gun != null && ammoText != null)
        {
            ammoText.text = gun.Ammo + " / " + gun.magSize;
            ammoText.color = gun.Ammo == 0 ? Color.red : Color.white;
        }
    }

    void UpdateHealth()
    {
        if (health == null) return;

        float maxHp = health.maxHealth > 0f ? health.maxHealth : 100f;
        float curHp = Mathf.Max(0f, health.Health);
        float pct = Mathf.Clamp01(curHp / maxHp);

        // Update health text
        if (healthText != null)
        {
            healthText.text = "HP " + Mathf.CeilToInt(curHp);
        }

        // Update health bar fill amount and smooth color gradient (green -> yellow -> red)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = pct;
            Color healthColor = Color.Lerp(Color.red, Color.green, pct);
            healthBarFill.color = healthColor;

            if (healthText != null)
            {
                healthText.color = curHp <= 30f ? Color.red : Color.white;
            }
        }

        // Death overlay message
        if (deathText != null)
        {
            deathText.gameObject.SetActive(health.IsDead);
        }
    }

    void UpdateCrosshair()
    {
        if (crosshairRoot == null) return;

        // Hide crosshair when player is dead or aiming down sights with a scope
        bool isAiming = inputRouter != null && inputRouter.AimHeld;
        bool isDead = health != null && health.IsDead;

        if (isAiming || isDead)
        {
            if (crosshairRoot.gameObject.activeSelf)
                crosshairRoot.gameObject.SetActive(false);
            return;
        }

        if (!crosshairRoot.gameObject.activeSelf)
            crosshairRoot.gameObject.SetActive(true);

        // Determine target spread based on stance and movement
        float targetSpread = baseSpread;

        if (playerController != null)
        {
            if (playerController.CurrentStance == PlayerController.Stance.Crouch)
            {
                targetSpread = crouchSpread;
            }
            else if (playerController.CurrentStance == PlayerController.Stance.Prone)
            {
                targetSpread = proneSpread;
            }
            else
            {
                // Standing: spread expands on movement
                if (inputRouter != null && inputRouter.Move.sqrMagnitude > 0.01f)
                {
                    targetSpread = inputRouter.Sprint ? sprintSpread : moveSpread;
                }
            }
        }

        // Smoothly interpolate current spread
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * spreadSpeed);

        // Position crosshair bars around center
        if (crosshairTop != null)
            crosshairTop.anchoredPosition = new Vector2(0f, currentSpread);
        if (crosshairBottom != null)
            crosshairBottom.anchoredPosition = new Vector2(0f, -currentSpread);
        if (crosshairLeft != null)
            crosshairLeft.anchoredPosition = new Vector2(-currentSpread, 0f);
        if (crosshairRight != null)
            crosshairRight.anchoredPosition = new Vector2(currentSpread, 0f);
    }

    /// <summary>
    /// Auto-generates a health bar with a green-to-red fill if none was assigned.
    /// </summary>
    void EnsureHealthBar()
    {
        if (healthBarFill != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Check if HealthBar root already exists
        Transform existingBar = canvas.transform.Find("HealthBar");
        if (existingBar != null)
        {
            Transform fillChild = existingBar.Find("Fill");
            if (fillChild != null)
            {
                healthBarFill = fillChild.GetComponent<Image>();
                return;
            }
        }

        // Create container in bottom-left
        GameObject barRoot = new GameObject("HealthBar", typeof(RectTransform));
        barRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = barRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.anchoredPosition = new Vector2(50f, 60f);
        rootRect.sizeDelta = new Vector2(250f, 20f);

        // Background (semi-transparent dark gray)
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(barRoot.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        healthBarBackground = bgObj.GetComponent<Image>();
        healthBarBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        // Fill image (Horizontal fill)
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(barRoot.transform, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        healthBarFill = fillObj.GetComponent<Image>();
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.color = Color.green;

        // If healthText is nearby, re-parent or adjust position
        if (healthText != null)
        {
            RectTransform textRect = healthText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0f, 0f);
            textRect.pivot = new Vector2(0f, 0f);
            textRect.anchoredPosition = new Vector2(50f, 85f);
        }
    }

    /// <summary>
    /// Auto-generates procedural 4-bar crosshair in the center of the screen if none was assigned.
    /// </summary>
    void EnsureCrosshair()
    {
        if (crosshairRoot != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Check if Crosshair already exists under Canvas
        Transform existing = canvas.transform.Find("Crosshair");
        if (existing != null)
        {
            crosshairRoot = existing.GetComponent<RectTransform>();
            crosshairTop = existing.Find("Top")?.GetComponent<RectTransform>();
            crosshairBottom = existing.Find("Bottom")?.GetComponent<RectTransform>();
            crosshairLeft = existing.Find("Left")?.GetComponent<RectTransform>();
            crosshairRight = existing.Find("Right")?.GetComponent<RectTransform>();
            return;
        }

        // Root crosshair container centered on screen
        GameObject rootGo = new GameObject("Crosshair", typeof(RectTransform));
        rootGo.transform.SetParent(canvas.transform, false);

        crosshairRoot = rootGo.GetComponent<RectTransform>();
        crosshairRoot.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRoot.pivot = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchoredPosition = Vector2.zero;
        crosshairRoot.sizeDelta = new Vector2(100f, 100f);

        Color crosshairColor = new Color(1f, 1f, 1f, 0.85f);
        float lineLength = 12f;
        float lineThickness = 3f;

        // Top bar
        crosshairTop = CreateCrosshairBar(rootGo.transform, "Top", new Vector2(lineThickness, lineLength), crosshairColor);
        // Bottom bar
        crosshairBottom = CreateCrosshairBar(rootGo.transform, "Bottom", new Vector2(lineThickness, lineLength), crosshairColor);
        // Left bar
        crosshairLeft = CreateCrosshairBar(rootGo.transform, "Left", new Vector2(lineLength, lineThickness), crosshairColor);
        // Right bar
        crosshairRight = CreateCrosshairBar(rootGo.transform, "Right", new Vector2(lineLength, lineThickness), crosshairColor);
    }

    RectTransform CreateCrosshairBar(Transform parent, string barName, Vector2 size, Color color)
    {
        GameObject bar = new GameObject(barName, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);

        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;

        Image img = bar.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        return rt;
    }
}
