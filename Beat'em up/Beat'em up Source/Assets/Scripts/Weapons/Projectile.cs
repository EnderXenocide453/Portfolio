using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 10;

    private float _damage = 2;
    private List<string> _targetTags;

    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);
    }

    public void SetParams(Vector3 target, List<string> targetTags, float damage)
    {
        transform.parent = null;

        _targetTags = targetTags;
        _damage = damage;
        
        Vector2 projectileDestination = (target - transform.position).normalized;
        transform.localScale = new Vector3(Mathf.Sign(projectileDestination.x), 1, 1);
        GetComponent<Rigidbody2D>().velocity = projectileDestination * Speed;

        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Sign(projectileDestination.x) * Mathf.Asin(projectileDestination.y / projectileDestination.magnitude) * Mathf.Rad2Deg));
        transform.localScale = new Vector3(Mathf.Sign(projectileDestination.x), 1, 1);
        
        StartCoroutine(StartDeath());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targetTags.Contains(collision.tag)) {
            print("Projectile hited the " + collision.tag);
            collision.GetComponent<CharacterController>().ChangeHP(-_damage);
            Destroy(gameObject);
        }
    }

    IEnumerator StartDeath() {
        yield return new WaitForSeconds(50/Speed);
        Destroy(gameObject);
        StopCoroutine(StartDeath());
    }
}
