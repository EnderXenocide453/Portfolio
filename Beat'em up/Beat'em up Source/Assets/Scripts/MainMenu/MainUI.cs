using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{

    public RectTransform ExitBTN, GameBTN, GameName;

    private void Start()
    {
        StartCoroutine(AnimationBTN());
    }


    public void Play()
    {
        SceneManager.LoadScene("ChooseMenu");
    }


    public void Exit()
    {
        Application.Quit();
    }


    IEnumerator AnimationBTN()
    {
        for (float i = -265; i <= 0; i += 5f)
        {
            ExitBTN.offsetMin = new Vector2(0, i);
            ExitBTN.offsetMax = new Vector2(0, i);
            GameBTN.offsetMin = new Vector2(0, i);
            GameBTN.offsetMax = new Vector2(0, i);
            GameName.offsetMin = new Vector2(0, -i);
            GameName.offsetMax = new Vector2(0, -i);

            yield return new WaitForSeconds(0.001f);
        }
    }
}
