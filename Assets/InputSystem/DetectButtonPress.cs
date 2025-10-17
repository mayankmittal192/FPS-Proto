using UnityEngine;
using UnityEngine.EventSystems;

public class DetectButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public InputSystemManager inputManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        inputManager.FireButtonPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputManager.FireButtonReleased();
    }
}
