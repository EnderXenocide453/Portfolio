using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintButton : MonoBehaviour
{
    //������ ��������� ���������
    public List<RectTransform> Hints;

    private bool _active = false;

    //������� ��������� ��� ������� �����
    void Start()
    {
        foreach (RectTransform hint in Hints)
            hint.gameObject.SetActive(false);
    }

    //�������� ��� �������� ���������
    public void SwitchHint()
    {
        foreach (RectTransform hint in Hints)
            hint.gameObject.SetActive(!_active);

        _active = !_active;
    }
}
