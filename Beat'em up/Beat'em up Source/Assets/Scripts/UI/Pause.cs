using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour, IPointerDownHandler
{

    public GameObject PauseBG;
    private bool _pause = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_pause) {
            Time.timeScale = 0;
            PauseBG.SetActive(true);
        } else {
            Time.timeScale = 1;
        }
        _pause = !_pause;
    }
    public void Play()
    {
        if (!_pause)
        {
            Time.timeScale = 0;
            PauseBG.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            PauseBG.SetActive(false);
        }
        _pause = !_pause;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("ChooseMenu");
        Time.timeScale = 1;
        _pause = !_pause;
    }

    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
        _pause = !_pause;
    }
}
