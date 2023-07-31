using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    public delegate void NonGroundedHandler();
    public NonGroundedHandler NonGrounded;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Arena")
            NonGrounded.Invoke();
    }
}
