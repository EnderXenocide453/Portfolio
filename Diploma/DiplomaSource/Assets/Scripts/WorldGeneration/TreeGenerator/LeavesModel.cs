using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesModel : MonoBehaviour
{
    public Sprite FarSprite;

    private MeshFilter _mesh;
    private MeshRenderer _meshRend;
    private SpriteRenderer _spriteRend;

    void Awake()
    {
        _mesh = GetComponent<MeshFilter>();
        _meshRend = GetComponent<MeshRenderer>();
        _spriteRend = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    //Функция отображения нормальной модели листвы при её попадании в радиус отображения
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag(TriggerType.LeavesTrigger.ToString()))
            DisableSprite();
    }

    //Функция отображения картинки листвы вместо модели при выходе из радиуса отображения
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TriggerType.LeavesTrigger.ToString()))
            EnableSprite();
    }

    //Функция генерации листвы через класс IcosphereGen
    public void GenerateLeaves(Vector3 pos, Vector3 scale, int subdivisionCount, bool hide)
    {
        Mesh mesh = IcosphereGen.GenerateIcosphere(subdivisionCount).CreateMesh();
        _mesh.mesh = mesh;

        SetPosition(pos);
        SetScale(scale);

        _meshRend.enabled = !hide;
        _spriteRend.enabled = hide;
    }

    //Включение картинки и отключение модели
    private void EnableSprite()
    {
        _meshRend.enabled = false;
        _spriteRend.enabled = true;
    }

    //Отключение картинки и включение модели
    private void DisableSprite()
    {
        _meshRend.enabled = true;
        _spriteRend.enabled = false;
    }

    //Установка позиции
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    //Установка масштаба
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
