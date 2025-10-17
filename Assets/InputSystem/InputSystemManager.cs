using Unity.Mathematics;
using UnityEngine;

public class InputSystemManager : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float lookSensitivityStandalone = 0.2f;
    public float lookSensitivityMobile = 0.8f;

    public static Vector2 Move;
    public static Vector2 Look;
    public static bool Fire;

    private InputSystem_Actions input;
    private float sensitivity;

    void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.Enable();
        input.UI.Enable();

#if UNITY_ANDROID && !UNITY_EDITOR
        sensitivity = lookSensitivityMobile;
#else
        sensitivity = lookSensitivityStandalone;
#endif
    }

    public void FireButtonPressed()
    {
        Fire = true;
    }

    public void FireButtonReleased()
    {
        Fire = false;
    }

    void Update()
    {
        Move = math.normalizesafe(input.Player.Move.ReadValue<Vector2>()) * moveSpeed;
        Look = input.Player.Look.ReadValue<Vector2>() * sensitivity;
#if !UNITY_ANDROID || UNITY_EDITOR
        Fire = input.Player.Attack.ReadValue<float>() > 0;
#endif
    }

    void OnDestroy()
    {
        input.Player.Disable();
        input.UI.Disable();
        input.Dispose();
    }
}
