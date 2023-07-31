using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choose : MonoBehaviour
{

    public string[] Heroes;
    public string[] Weapons;

    public bool ChooseWeapon = false;

    public Transform HeroCont;

    public UIAnim UIScript;

    private int charInt = 0;
    private int weapInt = 0;


    private void Start()
    {
        UIScript.ChangeSwitch += ChooseWeaponChange;
        UIScript.ToArena += SendChanges;

        ChangeHero(0);
    }

    public void Shift(int shift)
    {
        if (ChooseWeapon)
            ChangeWeapon(shift);
        else
            ChangeHero(shift);
    }

    public void ChangeHero(int shift)
    {
        charInt += shift;
        if (charInt >= Heroes.Length)
            charInt = 0;
        else if (charInt < 0)
            charInt = Heroes.Length-1;

        Destroy(HeroCont.GetChild(0).gameObject);
        Transform hero = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/" + Heroes[charInt]), HeroCont).transform;
        hero.transform.localPosition = Vector3.zero;

        hero.GetComponent<CharacterController>().ChangeWeapon(Weapons[weapInt]);
    }


    public void ChangeWeapon(int shift)
    {
        weapInt += shift;
        if (weapInt >= Weapons.Length)
            weapInt = 0;
        else if (weapInt < 0)
            weapInt = Weapons.Length - 1;

        HeroCont.GetChild(0).gameObject.GetComponent<CharacterController>().ChangeWeapon(Weapons[weapInt]);
    }

    public void ChooseWeaponChange()
    {
        ChooseWeapon = !ChooseWeapon;
    }

    private void SendChanges()
    {
        PlayerPrefs.SetString("hero", Heroes[charInt]);
        PlayerPrefs.SetString("weapon", Weapons[weapInt]);
    }
}

