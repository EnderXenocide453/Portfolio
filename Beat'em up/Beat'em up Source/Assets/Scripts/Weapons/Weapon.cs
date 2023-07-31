using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float Damage;
    public float AttackRange = 1;
    public float AttackAreaOffset = 0;
    public float AttackPerSecond;
    public bool IsMelee = true;
    public bool IsRange = false;
    public bool FreezeRotation = false;
    public ColliderType AttackType = ColliderType.HalfCircle;

    public delegate void AttackHandler();
    public AttackHandler AttackEnds;

    public Transform ProjectileSpawnPoint;
    public GameObject Projectile;

    private Animator _animator;
    
    private List<string> _enemyTags = new List<string>();
    private bool _attackAccess = true;
    private Quaternion _rotation = Quaternion.Euler(0, 0, 0);
    private Transform _parent;
    private Vector3 _targetPoint;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _parent = transform.parent;
        _enemyTags = _parent.parent.GetComponent<CharacterController>()._enemyTags;
    }

    public void Attack()
    {
        if (!_attackAccess) return;

        _animator.SetInteger("State", (int)AttackType);

        StartCoroutine(CoolDown());
    }

    public void AttackStart()
    {
        if (IsMelee)
            Punch();
        if (IsRange)
            Shoot();
    }

    public void AttackEnd()
    {
        if (IsMelee)
            AttackEnds.Invoke();

        _animator.SetInteger("State", -1);
    }

    private void Punch()
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Colliders/" + AttackType.ToString() + "Collider"), transform).GetComponent<DamageArea>().SetParams(AttackRange, Damage, _enemyTags, this, AttackAreaOffset);
    }

    private void Shoot()
    {
        Instantiate(Projectile, ProjectileSpawnPoint).GetComponent<Projectile>().SetParams(_targetPoint, _enemyTags, Damage);
    }

    public void SetDirection(Vector3 point)
    {
        Vector2 dist = point - transform.position;

        _rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Asin(dist.y / dist.magnitude) * Mathf.Rad2Deg));

        if (!FreezeRotation)
            _parent.localRotation = _rotation;

        _targetPoint = point;
    }
    private IEnumerator CoolDown()
    {
        _attackAccess = false;
        yield return new WaitForSeconds(1/AttackPerSecond);
        _attackAccess = true;
        StopCoroutine(CoolDown());
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (_isAttack && _enemyTags.Contains(collision.tag)) {
    //        collision.GetComponent<CharacterController>().ChangeHP(-Damage);
    //        _damagedIDs.Add(GetInstanceID());
    //        print(_damagedIDs.Count);
    //    }
    //}
}

public enum ColliderType
{
    HalfCircle,
    Circle,
    Rectangle,
    StuffBang,
    StuffZap
}