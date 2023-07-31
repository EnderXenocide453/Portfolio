using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    // Перемещение
    public float Speed = 4f;
    public Vector2 Direction = Vector2.zero;

    private bool _isFacingRight = true;
    private Rigidbody2D _body;
    private CheckGround _groundCheck;

    public Vector3 _currentTargetPoint { get; private set; } = Vector3.zero; //Координаты вейпоинта
    public List<string> _enemyTags { get; private set; } = new List<string>();

    // Параметры
    public float MaxHealth = 20;
    [SerializeField]
    public float Health;

    public delegate void StatHandler();
    public StatHandler HPHandler;

    // Бой
    public OwnerTypes Type = OwnerTypes.Neutral;
    
    private protected Weapon _weapon;
    private protected Transform _target;

    private float _minDist = -1;

    // Анимация
    private Animator _anim;

    private void Awake()
    {
        SetBehaviour(Type);
    }

    public virtual void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _weapon = transform.GetChild(0).GetChild(0).GetComponent<Weapon>();
        Health = MaxHealth;
        _anim = GetComponent<Animator>();
        _currentTargetPoint = transform.position;

        _groundCheck = Instantiate(Resources.Load<GameObject>("Prefabs/Colliders/CheckGround"), transform).GetComponent<CheckGround>();
        _groundCheck.transform.localPosition = Vector2.zero;
        _groundCheck.NonGrounded += StopMovement;
    }

    public virtual void FixedUpdate()
    {
        if (_weapon == null) {
            _weapon = transform.GetChild(0).GetChild(0).GetComponent<Weapon>();
        }

        _minDist = -1;

        _body.velocity = Direction * Speed;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 100);

        if (_target != null) {
            if (_target.position.x < transform.position.x && _isFacingRight || _target.position.x > transform.position.x && !_isFacingRight)
                Flip();
        } else if (Direction.x > 0 && !_isFacingRight || Direction.x < 0 && _isFacingRight)
            Flip();

        if (_target != null)
            _weapon.SetDirection(_target.position);
        else if (_body.velocity != Vector2.zero)
            _weapon.SetDirection(_body.velocity*10);

        SetTarget(null);
    }

    private void SetBehaviour(OwnerTypes type)
    {
        _enemyTags = new List<string>();
        Type = type;

        switch (Type) {
            case OwnerTypes.GoodGuy:
                _enemyTags.AddRange(new string[2] { "Enemy", "Structure" });
                break;
            case OwnerTypes.BadGuy:
                _enemyTags.AddRange(new string[3] { "Player", "Structure", "Friend" });
                break;
            case OwnerTypes.Genocidal:
                _enemyTags.AddRange(new string[4] { "Player", "Enemy", "Structure", "Friend" });
                break;
        }
    }

    public void SetDestination(Vector2 point)
    {
        Direction = point - new Vector2(transform.position.x, transform.position.y);
        if (Direction.magnitude > 1) Direction.Normalize();
        if (Direction.magnitude > 0.05f) {
            _anim.SetInteger("State", 1);
            return;
        }
        _anim.SetInteger("State", 0);
    }

    public void SetDirection(Vector2 dir)
    {
        Direction = dir;
        if (Direction.magnitude > 0.05f) {
            _anim.SetInteger("State", 1);
            return;
        }
        _anim.SetInteger("State", 0);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Attack()
    {
        if (_weapon != null)
        _weapon.Attack();
    }

    public virtual void ChangeHP(float hp)
    {
        Health += hp;
        print("[" + transform.name + "] Added health: " + hp);

        if (Health > MaxHealth) 
            Health = MaxHealth;
        else if (Health <= 0) 
            Death();
    }

    public virtual void Death(bool grave = true)
    {
        if (grave) {
            Destroy(_anim);
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Ambient/Grave");
            transform.localScale = Vector3.one;
            gameObject.layer = LayerMask.NameToLayer("Default");
            Destroy(transform.GetChild(0).gameObject);
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(this);
        } else
            Destroy(gameObject);
    }

    public virtual void LevitateDeath()
    {
        GetComponent<SpriteRenderer>().sortingOrder = -1;
        _weapon.GetComponent<SpriteRenderer>().sortingOrder = -1;
        Speed = 0;
        SetBehaviour(OwnerTypes.Neutral);

        StartCoroutine(Fall());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_enemyTags.Contains(collision.tag)) {
            float dist = Vector2.Distance(collision.transform.position, transform.position);
            if (_minDist > dist || _minDist == -1) {
                _minDist = dist;
                SetTarget(collision.transform);
                //MoveTo(_target.position);
            }
        }
    }

    public void MoveTo(Vector3 point)
    {
        _groundCheck.transform.localPosition = (point - transform.position).normalized * 0.5f * transform.localScale.x;

        _currentTargetPoint = point;
    }

    private void StopMovement()
    {
        MoveTo(transform.position * 0.8f);
    }

    public void ChangeWeapon(string name)
    {
        Transform weaponController = transform.GetChild(0);
        _weapon = null;

        if (weaponController.childCount > 0)
            Destroy(weaponController.GetChild(0).gameObject);

        Instantiate(Resources.Load("Prefabs/Weapons/" + name), weaponController);
        _weapon = transform.GetChild(0).GetChild(0).GetComponent<Weapon>();
    }

    private IEnumerator Fall()
    {
        for (int i = 0; i < 60; i++) {
            transform.position += Vector3.down * 0.08f;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + Vector3.forward * 0.75f);

            yield return new WaitForEndOfFrame();
        }
        Death(false);
        StopCoroutine(Fall());
    }
}

public enum OwnerTypes
{
    GoodGuy,
    BadGuy,
    Genocidal,
    Neutral
}