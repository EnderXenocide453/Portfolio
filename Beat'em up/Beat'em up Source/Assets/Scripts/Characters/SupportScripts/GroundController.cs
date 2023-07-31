using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    public CharacterController Owner;

    void Start()
    {
        Owner = transform.parent.GetComponent<CharacterController>();
    }
}
