using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : JumpingObject
{
    private void Update()
    {
        int horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        int vertical = (int)(Input.GetAxisRaw("Vertical"));

        if (vertical == 1)
        {
            VerticalJumpBy();
        }
        if (horizontal > 0 && transform.position.x < 1)
        {
            StartCoroutine(JumpBy(1, 0, 0));
        }
        if (horizontal < 0 && transform.position.x > -1)
        {
            StartCoroutine(JumpBy(-1, 0, 0));
        }
    }
    private void VerticalJumpBy()
    {
        if (!isJumping)
        {
            GameManager.instance.IncreaseJumps();
            StartCoroutine(JumpBy(0, 1, 1));
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Коля детектед!");
        if(other.tag == "Enemy")
        {
            GameManager.instance.GameOver();
        }
    }
}
