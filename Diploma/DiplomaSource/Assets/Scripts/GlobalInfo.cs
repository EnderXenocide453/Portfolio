using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalInfo
{
    private static float _inGameTime;

    public static float GetGameTime() => _inGameTime;

    public static IEnumerator TimeCount()
    {
        while (true) {
            yield return new WaitForSeconds(1);
            _inGameTime++;
        }
    }
}
