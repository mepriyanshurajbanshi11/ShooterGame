using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera tppCam, fppCam;
    public GameObject bodyModel;
    public float normalFov = 60f;
    public float[] scopeFovs = { 50f, 30f, 15f, 8f };
    public int scopeIndex = 0;

    PlayerInputRouter input;
    bool isFpp, isAiming;

    void Start()
    {
        input = GetComponent<PlayerInputRouter>();
        isFpp = PlayerPrefs.GetInt("fpp", 0) == 1;
        Apply();
    }

    void Update()
    {
        if (input.SwitchCamera) Toggle();
        if (input.AimHeld != isAiming) SetAim(input.AimHeld);
    }

    public void Toggle()
    {
        isFpp = !isFpp;
        PlayerPrefs.SetInt("fpp", isFpp ? 1 : 0);
        Apply();
    }

    void SetAim(bool aiming)
    {
        isAiming = aiming;
        Apply();
    }

    void Apply()
    {
        tppCam.enabled = !isFpp;
        fppCam.enabled = isFpp;
        bodyModel.SetActive(!isFpp);

        float fov = isAiming ? scopeFovs[scopeIndex] : normalFov;
        tppCam.fieldOfView = fov;
        fppCam.fieldOfView = fov;
    }
}