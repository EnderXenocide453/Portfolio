using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    public delegate void GroundedHandler(bool isGrounded);
    public GroundedHandler GroundedEvent;

    private void OnTriggerStay(Collider other)
    {
        GroundedEvent.Invoke(true);
        print("Grounded");
    }

    private void OnTriggerExit(Collider other)
    {
        GroundedEvent.Invoke(false);
        print("dickpic");
    }
}
