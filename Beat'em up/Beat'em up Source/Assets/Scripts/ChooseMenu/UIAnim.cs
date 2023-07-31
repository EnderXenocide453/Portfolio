using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIAnim : MonoBehaviour
{
    public RectTransform EquipmentField, Heroinfo;
    public GameObject WeaponButton, CharacterButton;
    public Transform Camera;

    public delegate void UIHandler();
    public UIHandler ChangeSwitch;
    public UIHandler ToArena;

    public void ToChooseWeapon ()
    {
        WeaponButton.SetActive(false);
        CharacterButton.SetActive(true);
        StartCoroutine(MoveCamera(2));
        StartCoroutine(MoveToChoose(2));

        ChangeSwitch.Invoke();
    }

    public void ToChooseCharacter ()
    {
        WeaponButton.SetActive(true);
        CharacterButton.SetActive(false);
        StartCoroutine(MoveCamera(1));
        StartCoroutine(MoveToChoose(1));

        ChangeSwitch.Invoke();
    }

    public void GameStart()
    {
        ToArena.Invoke();
        SceneManager.LoadScene("Arena");
    }

    public void Home()
    {
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator MoveToChoose(int movePlanes)
    {
        
        switch (movePlanes) {
            case 1:
                for (float i = 0; i >= -355f; i -= 5f)
                {
                    Heroinfo.offsetMin = new Vector2(i + 355, 0);
                    Heroinfo.offsetMax = new Vector2(i + 355, 0);
                    EquipmentField.offsetMin = new Vector2(i, 0);
                    EquipmentField.offsetMax = new Vector2(i, 0);
                    yield return new WaitForSeconds(0.001f);
                }

                break;
            case 2:
                for (float i = -355f; i <= 0; i += 5f)
                {
                    Heroinfo.offsetMin = new Vector2(i + 355, 0);
                    Heroinfo.offsetMax = new Vector2(i + 355, 0);
                    EquipmentField.offsetMin = new Vector2(i, 0);
                    EquipmentField.offsetMax = new Vector2(i, 0);
                    yield return new WaitForSeconds(0.001f);
                }
                break;
            default:
                Heroinfo.offsetMax = new Vector2(0, 0);
                Heroinfo.offsetMin = new Vector2(0, 0);
                EquipmentField.offsetMin = new Vector2(-335f, 0);
                EquipmentField.offsetMax = new Vector2(335f, 0);
                break;

        }
        
    }
    IEnumerator MoveCamera(int cameraSwitch)
    {
        switch (cameraSwitch) {
            
            case 1:
                for (float cameraSpeed = -1.05f; cameraSpeed <= 0f; cameraSpeed += 0.015f)
                {
                    Camera.position = new Vector2(cameraSpeed, 0f);
                    yield return new WaitForSeconds(0.001f);
                }
                break;

            case 2:
                for (float cameraSpeed = 0; cameraSpeed >= -1.05f; cameraSpeed -= 0.015f)
                {
                    Camera.position = new Vector2(cameraSpeed, 0f);
                    yield return new WaitForSeconds(0.001f);
                }
                break;
            default:
                Camera.position = new Vector2(0f, 0f);
                break;

        }
    }
}