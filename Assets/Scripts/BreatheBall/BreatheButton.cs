using UnityEngine;
using UnityEngine.EventSystems;

public class BreathButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public SquareBreathing2D breathingController;

    public void OnPointerDown(PointerEventData eventData)
    {
        breathingController.OnBreathButtonDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        breathingController.OnBreathButtonUp();
    }

    // Catches edge cases where finger slides off the button
    public void OnPointerExit(PointerEventData eventData)
    {
        breathingController.OnBreathButtonUp();
    }
}