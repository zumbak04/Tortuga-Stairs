using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : JumpingObject
{
    public bool IsJumpingForward { get; protected set; }

    private void Update()
    {
        int horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        int vertical = (int)(Input.GetAxisRaw("Vertical"));

        if (vertical == 1)
        {
            StartCoroutine(JumpForward());
        }
        if (horizontal > 0 && transform.position.x < 1)
        {
            StartCoroutine(JumpBy(1, 0, 0, 0.5f));
        }
        if (horizontal < 0 && transform.position.x > -1)
        {
            StartCoroutine(JumpBy(-1, 0, 0, 0.5f));
        }
    }
    private IEnumerator JumpForward()
    {
        if (!IsJumping)
        {
            IsJumpingForward = true;
            GameManager.instance.MoveForward();
            yield return StartCoroutine(JumpBy(0, 1, 1, 1));
            IsJumpingForward = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Коля детектед!");
        if(other.CompareTag("Enemy"))
        {
            GameManager.instance.GameOver();
        }
    }
}
