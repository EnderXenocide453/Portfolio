using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button ContinueButton, ExitButton;

    public bool pause { get; private set; } = false;

    //�������� ������� � �������� ������� �� ������
    private void Start()
    {
        ContinueButton.onClick.AddListener(delegate { Close(); });
        ExitButton.onClick.AddListener(delegate { Quit(); });
        gameObject.SetActive(false);
    }

    //����� �������� ���� �����
    public void Open()
    {
        pause = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //����� �������� ���� �����
    public void Close()
    {
        pause = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);
    }

    //����� ������ � ������� ����
    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
