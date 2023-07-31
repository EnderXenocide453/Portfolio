using System.Collections.Generic;

public class Fire : IEffect
{
    public string Name { get; set; } = "Fire";
    public EffectProperties Properties { get; set; }

    public void ApplyEffect()
    {
        throw new System.NotImplementedException();
    }
}
