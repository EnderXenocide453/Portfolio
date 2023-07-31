using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidebleObject : MonoBehaviour
{
    public TriggerType Trigger = TriggerType.ObjectTrigger;

    protected HidebleObjectContainer _container;

    //��� �������� �����
    protected virtual void Start()
    {
        //�������� ������������� ����������
        _container = new GameObject("container").AddComponent<HidebleObjectContainer>();
        _container.transform.position = transform.position;
        _container.transform.parent = transform.parent;
        transform.SetParent(_container.transform);

        //�������� ������� � �������� ������� � ���������
        _container.OnHide += Disable;
        _container.OnShow += Enable;

        _container.Trigger = Trigger;
        _container.StartWork();
    }

    //����������� ����� �������, ���������� � �������� ��������
    protected virtual void Disable()
    {

    }

    //����������� ����� ���������, ���������� � �������� ��������
    protected virtual void Enable()
    {

    }
}
