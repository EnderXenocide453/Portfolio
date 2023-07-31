using UnityEngine;
using System.Collections;

public class AICharacter : CharacterController
{
    //public float StopDistance = 1; //Дистанция до цели, на которой прекратится движение
    //public float AvoidArea = 0; //Дистанция до цели, при которой начнётся движение от неё
    public float WaitTime; // переменная для функции WaitForSeconds корутины
    private ArenaController _Wc;


    public override void Start()
    {
        _Wc = GameObject.Find("Arena").GetComponent<ArenaController>();
        //SetTarget(GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>());
        base.Start();
    }

    public override void FixedUpdate()
    {
        SetDestination(_currentTargetPoint);
        
        if (_target != null) {
            float distance = Vector2.Distance(transform.position, _currentTargetPoint);

            if (Vector2.Distance(transform.position, _target.position) <= _weapon.AttackRange) Attack();

            if (distance <= 0.1f) {
                if (_weapon.IsMelee)
                    MoveTo(new Vector2(_target.position.x, _target.position.y) + Random.insideUnitCircle * Random.Range(0.0f, 1.5f));
                else
                    MoveTo(new Vector2(transform.position.x, transform.position.y) + Random.insideUnitCircle * Random.Range(0.0f, 3f));
            }
        }

        base.FixedUpdate();
    }

    public override void Death(bool grave) {
        if (Type == OwnerTypes.BadGuy) {
            _Wc.EnemyDeath();
        }
        base.Death(grave);
    }

    public override void LevitateDeath()
    {
        if (Type == OwnerTypes.BadGuy) {
            _Wc.EnemyDeath();
        }
        base.LevitateDeath();
    }
}

/*
 Если враг находится на последней точке, то он берет новую координату нахождения персонажа, запоминает ее и идет к ней. 
 Если на этой координате есть наша цель начинает атаку, если нету то опять берет координату и идет к ней.
 ВО время того когда произошел удар, враг, пока идет "перезарядка" удара берет хаотичные координаты и следует к ним, они не должны быть очень большими.

 

Когда враг находится вне поля зрения, рандомно выбирается точка, к которой надо идти.
После достижения точки проходит рандомное время и выбирается новая точка.

Как только враг попадает в поле зрения, начинается движение к точке, где он был обнаружен, по достижении точки проходит немного времени (рандомно) и,
если враг всё ещё в поле зрения, действие повторяется.

Если враг попадает в радиус атаки, через небольшое время происходит атака.

Пока идёт откат атаки, персонаж держится в определенной области относительно цели и передвигается на небольшие рандомные промежутки.
 
 */