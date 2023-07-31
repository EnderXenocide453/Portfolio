using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    private float _damage;
    private List<string> _targetTags = new List<string>();
    private List<int> _damagedIDs = new List<int>();
    private Weapon _parentWeapon;

    public void SetParams(float range, float damage, List<string> tags, Weapon parentWeapon, float offset = 0)
    {
        _parentWeapon = parentWeapon;
        parentWeapon.AttackEnds += Death;

        _damage = damage;
        _targetTags = tags;

        transform.localScale *= range;
        transform.localPosition += Vector3.right * offset;
        transform.parent = null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_targetTags.Contains(collision.tag) && !_damagedIDs.Contains(collision.GetInstanceID())) {
            collision.GetComponent<CharacterController>().ChangeHP(-_damage);
            _damagedIDs.Add(collision.GetInstanceID());
        }
    }

    private void Death()
    {
        _parentWeapon.AttackEnds -= Death;
        Destroy(gameObject);
    }
}
