using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool OnlyY;
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(_camera.transform);
        if (OnlyY)
            transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
    }
}
