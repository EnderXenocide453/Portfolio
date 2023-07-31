using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidebleObjectContainer : MonoBehaviour
{
    //��� ��������, ����������� � ������������ ������
    public TriggerType Trigger = TriggerType.ObjectTrigger;

    //������� ������� � ���������
    public delegate void HideHendler();
    public event HideHendler OnHide;
    public event HideHendler OnShow;

    //��� ������������� ���������� ������ �� ��������� �����
    public void StartWork()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        gameObject.AddComponent<SphereCollider>().isTrigger = true;
    }

    //��� ��������������� � ���������, ���������� ������� ����������� �������
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag(Trigger.ToString())) {
            transform.GetChild(0).gameObject.SetActive(true);
            OnShow.Invoke();
        }
    }

    //��� ��������������� � ���������, ���������� ������� ������� �������
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Trigger.ToString())) {
            OnHide.Invoke();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
