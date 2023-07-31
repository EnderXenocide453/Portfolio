using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintButton : MonoBehaviour
{
    //Список элементов подсказки
    public List<RectTransform> Hints;

    private bool _active = false;

    //Скрытие подсказки при запуске сцены
    void Start()
    {
        foreach (RectTransform hint in Hints)
            hint.gameObject.SetActive(false);
    }

    //Открытие или закрытие подсказки
    public void SwitchHint()
    {
        foreach (RectTransform hint in Hints)
            hint.gameObject.SetActive(!_active);

        _active = !_active;
    }
}
