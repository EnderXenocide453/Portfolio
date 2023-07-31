using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidebleObjectContainer : MonoBehaviour
{
    //Тип триггера, включающего и выключающего объект
    public TriggerType Trigger = TriggerType.ObjectTrigger;

    //События скрытия и появления
    public delegate void HideHendler();
    public event HideHendler OnHide;
    public event HideHendler OnShow;

    //При инициализации скрываемый объект по умолчанию скрыт
    public void StartWork()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        gameObject.AddComponent<SphereCollider>().isTrigger = true;
    }

    //При соприкосновении с триггером, вызывается событие отображения объекта
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag(Trigger.ToString())) {
            transform.GetChild(0).gameObject.SetActive(true);
            OnShow.Invoke();
        }
    }

    //При соприкосновении с триггером, вызывается событие скрытия объекта
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Trigger.ToString())) {
            OnHide.Invoke();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
