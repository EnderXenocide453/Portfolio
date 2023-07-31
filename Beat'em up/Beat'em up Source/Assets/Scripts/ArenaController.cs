using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArenaController : MonoBehaviour
{
    // Методы контроля волн
    public int NumOfEmemies = 3;
    public int EnemyCount = 0;
    public int WaveCount = 0;
    public int NumOfWaves;
    public string[][] WaveEnemies;
    public EnemyTypes[] ListOfEnemies;
    public EnemyTypes CurrentEnemy;


    // Методы работы с коллайдером
    public Vector2 Size;
    public bool Ellipse = false;

    private void Awake()
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Characters/" + PlayerPrefs.GetString("hero"))).GetComponent<Player>().ChangeWeapon(PlayerPrefs.GetString("weapon"));
    }

    public virtual void Start()
    {
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        collider.points = GenerateEllipse(64).ToArray();

        SpawnWave(ListOfEnemies);
    }

    public void SpawnWave(EnemyTypes[] Enemies)
    {
        if(WaveCount >= NumOfWaves) return;
        for(int i = 0; i < NumOfEmemies; i++) {
            EnemyCount += 1;
            CurrentEnemy = Enemies[Random.Range(0, Enemies.Length)];
            Instantiate(Resources.Load<GameObject>("Prefabs/Characters/" + CurrentEnemy.Name), 
                Random.insideUnitCircle * Size / 2, Quaternion.identity).
                GetComponent<CharacterController>().ChangeWeapon(CurrentEnemy.Weapons[Random.Range(0, CurrentEnemy.Weapons.Length)]);
        }
    }

    public void RespawnPlayer()
    {
        SceneManager.LoadScene("ChooseMenu");
    }

    public void EnemyDeath() {
        EnemyCount--;
        
        if(EnemyCount == 0) {
            WaveCount++;
            NumOfEmemies += Random.Range(0, 3);
            SpawnWave(ListOfEnemies);
        }
        
    }

    public struct EnemyTypes {
        public string Name;
        public string[] Weapons;
    }

    private List<Vector2> GenerateEllipse(int segments)
    {
        List<Vector2> coordinates = new List<Vector2>();

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++) {
            Vector2 coordinate = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle) * Size.x / 2, Mathf.Cos(Mathf.Deg2Rad * angle) * Size.y / 2);
            angle += (360f / segments);

            coordinates.Add(coordinate);
        }

        return coordinates;
    }
}
