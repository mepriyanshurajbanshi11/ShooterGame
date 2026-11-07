using UnityEngine;

public class MobileControlsToggle : MonoBehaviour
{
    public bool forceShowInEditor = false;

    void Awake()
    {
        gameObject.SetActive(Application.isMobilePlatform || forceShowInEditor);
    }
}