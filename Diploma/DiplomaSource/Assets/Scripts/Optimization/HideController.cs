using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideController : MonoBehaviour
{
    //Радиусы отображения объектов
    public float ObjHideDistance;
    public float TreeHideDistance;
    public float ChunkHideDistance;
    public float FarTreeHideDistance;
    public float LeavesHideDistance;

    //Триггеры отображения объектов
    public SphereCollider ObjTrigger;
    public SphereCollider TreeTrigger;
    public SphereCollider LeavesTrigger;
    public SphereCollider ChunkTrigger;
    public SphereCollider FarTreeTrigger;

    //При запуске сцены генерируются триггеры и им присваиваются радиусы
    private void Start()
    {
        GameObject trigger = Resources.Load<GameObject>("Prefabs/Trigger");

        ObjTrigger = Instantiate(trigger, transform).GetComponent<SphereCollider>();
        ObjTrigger.gameObject.layer = gameObject.layer;
        ObjTrigger.radius = ObjHideDistance;
        ObjTrigger.tag = TriggerType.ObjectTrigger.ToString();

        TreeTrigger = Instantiate(trigger, transform).GetComponent<SphereCollider>();
        TreeTrigger.gameObject.layer = gameObject.layer;
        TreeTrigger.radius = WorldLoader.TreeRenderRadius / 4;
        TreeTrigger.tag = TriggerType.TreeTrigger.ToString();

        LeavesTrigger = Instantiate(trigger, transform).GetComponent<SphereCollider>();
        LeavesTrigger.gameObject.layer = gameObject.layer;
        LeavesTrigger.radius = WorldLoader.LeavesRenderRadius;
        LeavesTrigger.tag = TriggerType.LeavesTrigger.ToString();

        ChunkTrigger = Instantiate(trigger, transform).GetComponent<SphereCollider>();
        ChunkTrigger.gameObject.layer = gameObject.layer;
        ChunkTrigger.radius = WorldLoader.ChunkRenderRadius;
        ChunkTrigger.tag = TriggerType.ChunkTrigger.ToString();

        FarTreeTrigger = Instantiate(trigger, transform).GetComponent<SphereCollider>();
        FarTreeTrigger.gameObject.layer = gameObject.layer;
        FarTreeTrigger.radius = WorldLoader.TreeRenderRadius;
        FarTreeTrigger.tag = TriggerType.FarTreeTrigger.ToString();
    }
}

//Перечисление типов триггеров
public enum TriggerType
{
    ObjectTrigger,
    TreeTrigger,
    LeavesTrigger,
    GrassTrigger,
    CharacterTrigger,
    ChunkTrigger,
    FarTreeTrigger
}
