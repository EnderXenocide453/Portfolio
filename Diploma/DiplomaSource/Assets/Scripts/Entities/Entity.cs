using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

class Entity : MonoBehaviour
{
    //Stats variables
    [Header("Stats")]
    public float MaxHP = 100;
    private float _currentHP;

    //Effects variables
    private Dictionary<string, Coroutine> _currentEffects = new Dictionary<string, Coroutine>();

    protected virtual void Awake()
    {
        _currentHP = MaxHP;
    }

    private void Start()
    {
        AddEffect(new EntityEffect("damage", 0.1f, ChangeHP, -50, EntityEffectType.permanent, ChangeHP, -25));
        AddEffect(new EntityEffect("heal", 2, ChangeHP, 1, EntityEffectType.cyclic));
        AddEffect(new EntityEffect("heal", 3, ChangeHP, 0.5f, EntityEffectType.cyclic));

        print(_currentEffects.Keys.Count);
    }

    protected virtual void ChangeHP(float hp)
    {
        _currentHP += hp;
        if (_currentHP > MaxHP)
            _currentHP = MaxHP;
        else if (_currentHP <= 0)
            Death();
        print(_currentHP);
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }

    //Effects
    protected virtual void AddEffect(EntityEffect effect) // Сюда передавать инфу о партиклах, цвете и функции эффекта
    {
        IEnumerator enumerator;

        if (effect.Type == 0)
            enumerator = PermanentEffect(effect);
        else
            enumerator = CyclicEffect(effect);

        if (_currentEffects.ContainsKey(effect.Name))
            RemoveEffect(effect.Name);
        _currentEffects.Add(effect.Name, StartCoroutine(enumerator));
    }

    protected virtual void RemoveEffect(string effect) 
    {
        StopCoroutine(_currentEffects[effect]);
        _currentEffects.Remove(effect);
    }

    protected virtual IEnumerator CyclicEffect(EntityEffect effect)
    {
        for (int i = 0; i < effect.Duration / effect.CycleCooldown; i++) {
            effect.Func(effect.Value);
            yield return new WaitForSeconds(effect.CycleCooldown);
        }

        effect.EndFunc?.Invoke(effect.EndValue);

        RemoveEffect(effect.Name);
    }

    protected virtual IEnumerator PermanentEffect(EntityEffect effect)
    {
        effect.Func(effect.Value);
        yield return new WaitForSeconds(effect.Duration);

        effect.EndFunc?.Invoke(effect.EndValue);
        RemoveEffect(effect.Name);
    }
}

public struct EntityEffect
{
    public delegate void EffectFunc(float value);

    public string Name;
    public float Duration;

    public float Value;
    public float EndValue;
    public EffectFunc Func;
    public EffectFunc EndFunc;

    public EntityEffectType Type;
    public float CycleCooldown;

    public EntityEffect(string name, float duration, EffectFunc func, float value, EntityEffectType type, EffectFunc endFunc = null, float endValue = 0, float cooldown = 0.1f)
    {
        Name = name;
        Duration = duration;
        Value = value;
        Func = func;
        EndValue = endValue;
        EndFunc = endFunc;
        Type = type;
        CycleCooldown = cooldown;
    }
}

public enum EntityEffectType
{
    permanent,
    cyclic
}