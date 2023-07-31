using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidebleObject : MonoBehaviour
{
    public TriggerType Trigger = TriggerType.ObjectTrigger;

    protected HidebleObjectContainer _container;

    //При загрузке сцены
    protected virtual void Start()
    {
        //Создание родительского контейнера
        _container = new GameObject("container").AddComponent<HidebleObjectContainer>();
        _container.transform.position = transform.position;
        _container.transform.parent = transform.parent;
        transform.SetParent(_container.transform);

        //Привязка методов к событиям скрытия и появления
        _container.OnHide += Disable;
        _container.OnShow += Enable;

        _container.Trigger = Trigger;
        _container.StartWork();
    }

    //Виртуальный метод скрытия, изменяемый в дочерних объектах
    protected virtual void Disable()
    {

    }

    //Виртуальный метод появления, изменяемый в дочерних объектах
    protected virtual void Enable()
    {

    }
}
