using UnityEngine;

public class Player : CharacterController
{
    public StatHandler ManaHandler;
    public StatHandler StaminaHandler;

    public float MaxMana;
    public float Mana;
    public float MaxStamina;
    public float Stamina;
    
    private Joystick joystick;

    public override void Start()
    {
        base.Start();
        joystick = GameObject.Find("InnerStick").GetComponent<Joystick>();
        GameObject.Find("AttackButton").GetComponent<BattleButton>().PressEvent += Attack;
    }

    public override void FixedUpdate()
    {
        SetDirection(joystick.Direction);
        base.FixedUpdate();
    }

    public override void ChangeHP(float hp)
    {
        base.ChangeHP(hp);
        HPHandler.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Attack();
    }

    public override void Death(bool grave)
    {
        GameObject.Find("Arena").GetComponent<ArenaController>().RespawnPlayer();
        base.Death(grave);
    }

    public override void LevitateDeath()
    {
        base.LevitateDeath();
    }
}