using System.Collections.Generic;
using UnityEngine;

public interface IEffect
{
    public string Name { get; set; }
    public EffectProperties Properties { get; set; }

    void ApplyEffect();
}

public enum Property
{
    Element,
    Shape,
    Target,
    Area,
    Color,
    Particles
}

public enum SpellElement
{
    //Basic elements
    Fire,
    Water,
    Air,
    Earth,
    Cold,
    Electricity,
    Life,
    Death,
    Mana,
    Void,
    Light,
    Dark,
    //Combined elements
    Ice,
    Steam //...
}

public enum SpellTarget
{
    OnSelf,
    OnCreature,
    OnFoe,
    OnFriend,
    OnEverything
}

public enum SpellShape
{
    Ray,
    Flow,
    Arrow,
    Shield,
    Entity,
    Hail,
    Seal
}

public struct EffectProperties
{
    public SpellElement Element;
    public SpellTarget Target;
    public SpellShape Shape;
    public bool Area;
    public float AreaRange;
    public Color Color;
    //Particles

    public EffectProperties(SpellElement element, SpellTarget target, SpellShape shape, Color color, bool area = false, float range = 5f)
    {
        Element = element;
        Target = target;
        Shape = shape;
        Color = color;
        Area = area;
        AreaRange = range;
    }
}