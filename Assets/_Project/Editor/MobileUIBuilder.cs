#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MobileUIBuilder
{
    // Screen reference resolution matching target mobile aspect ratio (16:9 1080p)
    private static readonly Vector2 referenceResolution = new Vector2(1920f, 1080f);
    private const float canvasMatchRatio = 0.5f;

    // LookArea occupies the right 70% of the screen so it does not overlap the joystick area on the left
    private const float lookAreaAnchorMinX = 0.3f;

    // Joystick dimensions and position in bottom-left corner
    private static readonly Vector2 joystickPosition = new Vector2(200f, 200f);
    private static readonly Vector2 joystickBgSize = new Vector2(240f, 240f);
    private static readonly Vector2 joystickHandleSize = new Vector2(80f, 80f);
    private const float joystickRadius = 100f;

    // Button layout definition to keep parameters structured and avoid magic numbers
    private struct ButtonConfig
    {
        public string name;
        public HoldButton.Kind kind;
        public string label;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;
        public Vector2 size;
        public Color color;

        public ButtonConfig(
            string name,
            HoldButton.Kind kind,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            this.name = name;
            this.kind = kind;
            this.label = label;
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.anchoredPosition = anchoredPosition;
            this.size = size;
            this.color = color;
        }
    }

    [MenuItem("Tools/Build Mobile Controls")]
    public static void BuildMobileControls()
    {
        Canvas canvas = FindOrCreateCanvas();
        if (canvas == null)
        {
            Debug.LogError("[MobileUIBuilder] Failed to find or create Canvas.");
            return;
        }

        EnsureEventSystem();
        RemoveOldMobileControls(canvas);

        GameObject mobileControls = CreateMobileControlsRoot(canvas);
        if (mobileControls == null)
        {
            Debug.LogError("[MobileUIBuilder] Failed to create MobileControls root object.");
            return;
        }

        Sprite knobSprite = GetKnobSprite();
        Font defaultFont = GetDefaultFont();

        // LookArea must be created first so it stays behind interactive buttons in hierarchy raycasting
        CreateLookArea(mobileControls);

        // Movement joystick placed in bottom-left corner
        CreateJoystick(mobileControls, knobSprite);

        // Free Fire-style action buttons on the right side
        CreateButtons(mobileControls, knobSprite, defaultFont);

        // Notify Unity of scene modification so changes can be persisted
        if (canvas.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        Debug.Log("[MobileUIBuilder] Mobile controls built successfully!");
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        }

        // Standardize canvas scaling across different mobile device aspect ratios
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = canvasMatchRatio;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        }
    }

    private static void RemoveOldMobileControls(Canvas canvas)
    {
        // Iterate backwards so destroying elements does not shift unvisited child indices
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.name == "MobileControls")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }

    private static GameObject CreateMobileControlsRoot(Canvas canvas)
    {
        GameObject mobileControls = new GameObject("MobileControls", typeof(RectTransform));
        mobileControls.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(mobileControls, "Create MobileControls");

        // Stretched across full screen so child controls anchor cleanly relative to screen bounds
        RectTransform rect = mobileControls.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Force visibility in Unity Editor so the user can test immediately with mouse clicks
        MobileControlsToggle toggle = mobileControls.AddComponent<MobileControlsToggle>();
        toggle.forceShowInEditor = true;

        return mobileControls;
    }

    private static void CreateLookArea(GameObject parent)
    {
        GameObject lookArea = new GameObject("LookArea", typeof(RectTransform));
        lookArea.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(lookArea, "Create LookArea");

        // Covers right 70% of the screen, leaving left 30% solely for joystick dragging
        RectTransform rect = lookArea.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(lookAreaAnchorMinX, 0f);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Fully transparent raycast target to capture drag gestures across the right screen area
        Image img = lookArea.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = true;

        lookArea.AddComponent<TouchLookArea>();
    }

    private static void CreateJoystick(GameObject parent, Sprite knobSprite)
    {
        GameObject joystick = new GameObject("Joystick", typeof(RectTransform));
        joystick.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(joystick, "Create Joystick");

        RectTransform joyRect = joystick.GetComponent<RectTransform>();
        joyRect.anchorMin = Vector2.zero;
        joyRect.anchorMax = Vector2.zero;
        joyRect.pivot = new Vector2(0.5f, 0.5f);
        joyRect.anchoredPosition = joystickPosition;
        joyRect.sizeDelta = joystickBgSize;

        // Joystick background acts as visual reference and touch boundary
        GameObject bgGo = new GameObject("JoystickBG", typeof(RectTransform));
        bgGo.transform.SetParent(joystick.transform, false);
        Undo.RegisterCreatedObjectUndo(bgGo, "Create JoystickBG");

        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = joystickBgSize;

        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = knobSprite;
        bgImg.color = new Color(1f, 1f, 1f, 0.3f);
        bgImg.raycastTarget = true;

        // Inner knob that tracks user drag offset
        GameObject handleGo = new GameObject("JoystickHandle", typeof(RectTransform));
        handleGo.transform.SetParent(joystick.transform, false);
        Undo.RegisterCreatedObjectUndo(handleGo, "Create JoystickHandle");

        RectTransform handleRect = handleGo.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = joystickHandleSize;

        Image handleImg = handleGo.AddComponent<Image>();
        handleImg.sprite = knobSprite;
        handleImg.color = new Color(1f, 1f, 1f, 0.8f);
        // Handle raycastTarget false allows drag events to pass cleanly to the background
        handleImg.raycastTarget = false;

        VirtualJoystick vj = joystick.AddComponent<VirtualJoystick>();
        vj.background = bgRect;
        vj.handle = handleRect;
        vj.radius = joystickRadius;
    }

    private static void CreateButtons(GameObject parent, Sprite knobSprite, Font font)
    {
        // Free Fire style button layout configured with comfortable touch targets and non-overlapping margins
        ButtonConfig[] buttons = new ButtonConfig[]
        {
            new ButtonConfig("BtnFire", HoldButton.Kind.Fire, "FIRE",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 150f), new Vector2(100f, 100f),
                new Color(0.9f, 0.25f, 0.25f, 0.65f)), // Red-tinted

            new ButtonConfig("BtnAim", HoldButton.Kind.Aim, "AIM",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-270f, 150f), new Vector2(100f, 100f),
                new Color(0.95f, 0.85f, 0.2f, 0.65f)), // Yellow-tinted

            new ButtonConfig("BtnJump", HoldButton.Kind.Jump, "JUMP",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 270f), new Vector2(80f, 80f),
                new Color(0.25f, 0.85f, 0.35f, 0.65f)), // Green-tinted

            new ButtonConfig("BtnReload", HoldButton.Kind.Reload, "RELOAD",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-270f, 270f), new Vector2(80f, 80f),
                new Color(0.25f, 0.55f, 0.95f, 0.65f)), // Blue-tinted

            new ButtonConfig("BtnCrouch", HoldButton.Kind.Crouch, "CROUCH",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-375f, 270f), new Vector2(70f, 70f),
                new Color(0.6f, 0.6f, 0.6f, 0.65f)), // Gray

            new ButtonConfig("BtnProne", HoldButton.Kind.Prone, "PRONE",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-470f, 270f), new Vector2(70f, 70f),
                new Color(0.5f, 0.5f, 0.5f, 0.65f)), // Gray

            new ButtonConfig("BtnCamera", HoldButton.Kind.Camera, "CAM",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -80f), new Vector2(70f, 70f),
                new Color(0.65f, 0.65f, 0.65f, 0.65f)) // Gray
        };

        for (int i = 0; i < buttons.Length; i++)
        {
            CreateSingleButton(parent, buttons[i], knobSprite, font);
        }
    }

    private static void CreateSingleButton(GameObject parent, ButtonConfig config, Sprite knobSprite, Font font)
    {
        GameObject buttonGo = new GameObject(config.name, typeof(RectTransform));
        buttonGo.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(buttonGo, "Create " + config.name);

        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = config.anchorMin;
        rect.anchorMax = config.anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = config.anchoredPosition;
        rect.sizeDelta = config.size;

        Image img = buttonGo.AddComponent<Image>();
        img.sprite = knobSprite;
        img.color = config.color;
        img.raycastTarget = true;

        HoldButton hold = buttonGo.AddComponent<HoldButton>();
        hold.kind = config.kind;

        // Label child object to render button action text
        GameObject labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(buttonGo.transform, false);
        Undo.RegisterCreatedObjectUndo(labelGo, "Create " + config.name + " Label");

        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text text = labelGo.AddComponent<Text>();
        text.text = config.label;
        if (font != null)
        {
            text.font = font;
        }
        else
        {
            Debug.LogError("[MobileUIBuilder] Font is null while assigning text for " + config.name);
        }

        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 20;
        // Text raycastTarget false ensures the parent Image receives the touch events reliably
        text.raycastTarget = false;
    }

    private static Sprite GetKnobSprite()
    {
        // Built-in circular knob sprite produces clean rounded touch buttons
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (knob == null)
        {
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        return knob;
    }

    private static Font GetDefaultFont()
    {
        // Unity 2022+ / Unity 6 legacy text font location
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        // Backward compatibility fallback for older runtime environments
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            return font;
        }

        // Final fallback: fetch any loaded system font in editor
        Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
        if (fonts != null && fonts.Length > 0)
        {
            return fonts[0];
        }

        Debug.LogError("[MobileUIBuilder] Failed to locate a valid font for button labels.");
        return null;
    }
}
#endif
