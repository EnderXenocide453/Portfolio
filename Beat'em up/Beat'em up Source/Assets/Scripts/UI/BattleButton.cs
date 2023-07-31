using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BattleButton : MonoBehaviour, IPointerDownHandler
{
    public delegate void PressIventHandler();
    public PressIventHandler PressEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        PressEvent.Invoke();
    }
}
