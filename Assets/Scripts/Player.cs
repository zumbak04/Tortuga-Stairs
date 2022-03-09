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
        if (Input.GetMouseButtonDown(0))
        {
            JumpBy(0, 1, 1);
        }
    }
    private void JumpBy(float x, float y, float z)
    {
        if (!isJumping)
        {
            isJumping = true;
            GameManager.instance.IncreaseJumps();
            Vector3 target = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
            StartCoroutine(SmoothJump(target));
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
