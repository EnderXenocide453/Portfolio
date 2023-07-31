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

    //������� ����������� ���������� ������ ������ ��� � ��������� � ������ �����������
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag(TriggerType.LeavesTrigger.ToString()))
            DisableSprite();
    }

    //������� ����������� �������� ������ ������ ������ ��� ������ �� ������� �����������
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TriggerType.LeavesTrigger.ToString()))
            EnableSprite();
    }

    //������� ��������� ������ ����� ����� IcosphereGen
    public void GenerateLeaves(Vector3 pos, Vector3 scale, int subdivisionCount, bool hide)
    {
        Mesh mesh = IcosphereGen.GenerateIcosphere(subdivisionCount).CreateMesh();
        _mesh.mesh = mesh;

        SetPosition(pos);
        SetScale(scale);

        _meshRend.enabled = !hide;
        _spriteRend.enabled = hide;
    }

    //��������� �������� � ���������� ������
    private void EnableSprite()
    {
        _meshRend.enabled = false;
        _spriteRend.enabled = true;
    }

    //���������� �������� � ��������� ������
    private void DisableSprite()
    {
        _meshRend.enabled = true;
        _spriteRend.enabled = false;
    }

    //��������� �������
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    //��������� ��������
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
