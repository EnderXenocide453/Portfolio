using UnityEngine;

public class Joystick : MonoBehaviour
{
    private RectTransform _rect;
    private bool _hold;
    private int _touchID;
    private Vector2 _startPos;

    public Vector2 Direction { get; private set; } = Vector2.zero;
    public float Radius = 100;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
        _startPos = _rect.localPosition;
    }

    private void FixedUpdate()
    {
        //for (int i = 0; i < Input.touchCount - 1; i++) {
        //    Vector2 touchPos = Input.GetTouch(i).position;
        //    if ((touchPos - _startPos).magnitude <= 200) {
        //        _rect.anchoredPosition = touchPos - _startPos;
        //        if (_rect.anchoredPosition.magnitude > Radius) _rect.anchoredPosition = _rect.anchoredPosition.normalized * Radius;

        //        Direction = _rect.anchoredPosition / Radius;
        //        break;
        //    }
        //}

        if (Input.touchCount > 0) {
            _rect.anchoredPosition = new Vector2(Input.GetTouch(0).position.x * 1280 / Screen.width, Input.GetTouch(0).position.y * 720 / Screen.height) - _startPos;
            if (_rect.anchoredPosition.magnitude > Radius)
                _rect.anchoredPosition = _rect.anchoredPosition.normalized * Radius;

            Direction = _rect.anchoredPosition / Radius;
        } else {
            _rect.anchoredPosition = Vector2.zero;
            Direction = Vector2.zero;
        }
    }
}
