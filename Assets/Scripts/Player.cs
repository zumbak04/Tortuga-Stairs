using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : JumpingObject
{
    public bool IsJumpingForward { get; protected set; }
    Vector2 firstTouchPos;
    Vector2 secondTouchPos;

    private void Update()
    {
        int horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        int vertical = (int)(Input.GetAxisRaw("Vertical"));

        if (vertical == 1)
        {
            StartCoroutine(JumpForward());
        }
        if (horizontal > 0)
        {
            StartCoroutine(JumpBy(1, 0, 0, 0.5f));
        }
        if (horizontal < 0)
        {
            StartCoroutine(JumpBy(-1, 0, 0, 0.5f));
        }
        if (Input.touches.Length > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firstTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                secondTouchPos = touch.position;
                Vector2 deltaTouch = secondTouchPos - firstTouchPos;

                if (deltaTouch.magnitude > 20)
                {
                    if (deltaTouch.normalized.x > 0.5f)
                    {
                        StartCoroutine(JumpBy(1, 0, 0, 0.5f));
                    }
                    else if(deltaTouch.normalized.x < -0.5f)
                    {
                        StartCoroutine(JumpBy(-1, 0, 0, 0.5f));
                    }
                }
                else
                {
                    StartCoroutine(JumpForward());
                }
            }
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
        if(other.CompareTag("Enemy"))
        {
            GameManager.instance.GameOver();
        }
    }
}
