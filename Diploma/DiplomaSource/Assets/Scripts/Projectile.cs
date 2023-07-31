using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _speed;
    private float _gravityScale;
    private bool _area;
    private float _areaRange;

    private LayerMask _target;
    private EntityEffect _effect;

    public void SetProperties(float speed, EntityEffect effect, LayerMask target, float gravityScale = 0f, bool area = false, float areaRange = 5f)
    {
        _speed = speed;
        _effect = effect;
        _target = target;
        _gravityScale = gravityScale;
        _area = area;
        _areaRange = areaRange;
    }


}
