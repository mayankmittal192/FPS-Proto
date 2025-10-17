using UnityEngine;

public class ShowMobileControls : MonoBehaviour
{
    public RectTransform leftJoystick;
    public RectTransform rightJoystick;
    public RectTransform fireButton;

    public void ShowGamepadControlsIfMobile()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        leftJoystick.gameObject.SetActive(true);
        rightJoystick.gameObject.SetActive(true);
        fireButton.gameObject.SetActive(true);
#else
        leftJoystick.gameObject.SetActive(false);
        rightJoystick.gameObject.SetActive(false);
        fireButton.gameObject.SetActive(false);
#endif
    }
}
