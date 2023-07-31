using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldLoaderButton : MonoBehaviour
{
    public InputField SeedField; //Текстовое поле, в которое вводится значение зерна генерации
    public Button StartButton, SettingsButton, QuitButton, BackButton; //Ссылки на кнопки меню
    public RectTransform MainMenu, Settings; //ссылки на окна меню

    //При запуске сцены к событиям нажатия на кнопки привязываются соответственные методы
    private void Start()
    {
        SeedField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidateSeedChar(addedChar); };
        StartButton.onClick.AddListener(delegate { LoadWorld(Settings.GetComponent<SettingsController>()); });
        SettingsButton.onClick.AddListener(delegate { OpenSettings(); });
        QuitButton.onClick.AddListener(delegate { Exit(); });
        BackButton.onClick.AddListener(delegate { CloseSettings(); });
    }

    //Проверка вводимого в поле зерна генерации числа на типу Int32
    //вызывается при изменении значения в поле
    private char ValidateSeedChar(char c)
    {
        if (!Int32.TryParse(SeedField.text + c, out int num))
            c = '\0';

        return c;
    }

    public void LoadWorld(SettingsController settings) //Функция запуска генерации
    {
        if (SeedField.text.Length == 0) { //Если строка пуста, то генерируется случайное зерно
            WorldLoader.LoadWorld(UnityEngine.Random.Range(0, int.MaxValue), settings);
            return;
        }

        int seed = Convert.ToInt32(SeedField.text); //Конвертация значения в целое число
        WorldLoader.LoadWorld(seed,settings); //Запуск генерации
    }

    //Функция выхода из приложения
    public void Exit() => Application.Quit();

    //Функция открытия меню настроек
    public void OpenSettings()
    {
        MainMenu.gameObject.SetActive(false);
        Settings.gameObject.SetActive(true);
    }

    //Функция закрытия меню настроек
    public void CloseSettings()
    {
        Settings.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(true);
    }
}
