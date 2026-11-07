using UnityEngine;

/// <summary>
/// A single pooled floating damage number facing the camera.
/// Uses built-in TextMesh (no TextMeshPro required).
/// </summary>
public class DamageNumber : MonoBehaviour
{
    private TextMesh textMesh;
    private MeshRenderer meshRenderer;
    private float lifetime = 0.8f;
    private float floatSpeed = 1.4f;
    private float timer;
    private Color baseColor;
    private Camera activeCam;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null) textMesh = gameObject.AddComponent<TextMesh>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Setup crisp font rendering
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            textMesh.font = font;
            meshRenderer.material = font.material;
        }

        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.05f;
    }

    public void Spawn(Vector3 worldPos, float damage, bool isHeadshot, Camera cam)
    {
        activeCam = cam != null ? cam : Camera.main;

        // Position slightly above hit point with slight random horizontal offset to prevent stacking
        Vector3 offset = new Vector3(Random.Range(-0.15f, 0.15f), 0.35f, Random.Range(-0.15f, 0.15f));
        transform.position = worldPos + offset;

        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
            textMesh.fontSize = isHeadshot ? 52 : 36;
            baseColor = isHeadshot ? Color.yellow : Color.white;
            textMesh.color = baseColor;
        }

        timer = 0f;
        gameObject.SetActive(true);
        FaceCamera();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / lifetime);

        // Float upwards
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

        // Face the camera directly
        FaceCamera();

        // Fade out alpha over lifetime
        if (textMesh != null)
        {
            Color c = baseColor;
            c.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = c;
        }

        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
        }
    }

    void FaceCamera()
    {
        if (activeCam == null) activeCam = Camera.main;
        if (activeCam != null)
        {
            transform.forward = activeCam.transform.forward;
        }
    }
}
