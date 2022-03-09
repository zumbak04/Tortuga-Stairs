using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isJumping = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
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
            JumpBy(horizontal, 0, 0);
        }
        if (horizontal < 0 && transform.position.x > -1)
        {
            JumpBy(horizontal, 0, 0);
        }
    }
    private void JumpBy(float x, float y, float z)
    {
        if (!isJumping)
        {
            isJumping = true;
            Vector3 target = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
            StartCoroutine(SmoothJump(target));
        }
    }
    private void VerticalJumpBy()
    {
        if (!isJumping)
        {
            GameManager.instance.IncreaseJumps();
            JumpBy(0, 1, 1);
        }
    }
    private IEnumerator SmoothJump(Vector3 target)
    {
        Vector3 maxPoint = (target + transform.position)/2 + Vector3.up;
        while ((transform.position - maxPoint).sqrMagnitude > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, maxPoint, GameManager.instance.inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        while ((transform.position - target).sqrMagnitude > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target, GameManager.instance.inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        isJumping = false;
    }
}
