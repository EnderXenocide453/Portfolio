using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitateArenaController : ArenaController
{
    

    public override void Start()
    {
        //WaveEnemies = new string[][] {
        //    new string[] {"Warrior", "Warrior", "Mage", "Mage"},
        //    new string[] {"Warrior", "Warrior", "Warrior", "Warrior"},
        //    new string[] { "Mage", "Mage", "Mage", "Mage"},
        //    new string[] { "Mage", "Mage", "Mage", "Mage"}
        //};

        EnemyTypes Warrior = new EnemyTypes() {
            Name = "Warrior",
            Weapons = new string[] { "Battle ax", "Cleaver", "Sword" }
        };

        EnemyTypes Mage = new EnemyTypes() {
            Name = "Mage",
            Weapons = new string[] { "GreenStuff", "Stuff" }
        };

        ListOfEnemies = new EnemyTypes[] { Warrior, Mage };
        NumOfWaves = 4;

        base.Start();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GroundController")) {
            collision.GetComponent<GroundController>().Owner.LevitateDeath();
        }
    }

}
