using UnityEngine;

public class StatusBar : MonoBehaviour
{
    private Player _player;
    
    private RectTransform _health;
    private RectTransform _mana;
    private RectTransform _stamina;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _health = transform.GetChild(0).GetComponent<RectTransform>();
        _mana = transform.GetChild(1).GetComponent<RectTransform>();
        _stamina = transform.GetChild(2).GetComponent<RectTransform>();

        _player.HPHandler += UpdateHP;
        _player.ManaHandler += UpdateMP;
        _player.StaminaHandler += UpdateStamina;

        UpdateHP();
        UpdateMP();
        UpdateStamina();
    }

    public void UpdateHP()
    {
        _health.localScale = new Vector3(_player.Health / _player.MaxHealth, 1, 1);
    }

    public void UpdateMP()
    {
        _mana.localScale = new Vector3(_player.Mana / _player.MaxMana, 1, 1);
    }

    public void UpdateStamina()
    {
        _stamina.localScale = new Vector3(_player.Stamina / _player.MaxStamina, 1, 1);
    }
}
