using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingObject : MonoBehaviour
{
    public float jumpTime;

    protected float inverseJumpTime;

    public bool IsJumping { get; protected set; }

    protected virtual void Start()
    {
        inverseJumpTime = 1f / jumpTime;
    }
    protected IEnumerator JumpBy(float x, float y, float z, float heigh)
    {
        if (!IsJumping && (transform.position.x + x <= 1) && (transform.position.x + x >= -1))
        {
            IsJumping = true;

            Vector3 target = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z);
            Vector3 maxPoint = (target + transform.position) / 2 + Vector3.up * heigh;

            while ((transform.position - maxPoint).sqrMagnitude > float.Epsilon)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, maxPoint, inverseJumpTime * Time.deltaTime);
                transform.Rotate(new Vector3(z, 0, 0) * inverseJumpTime * Time.deltaTime * 45);
                transform.position = newPosition;
                yield return null;
            }
            while ((transform.position - target).sqrMagnitude > float.Epsilon)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, target, inverseJumpTime * Time.deltaTime);
                transform.Rotate(new Vector3(z, 0, 0) * inverseJumpTime * Time.deltaTime * 45);
                transform.position = newPosition;
                yield return null;
            }

            IsJumping = false;
        }
    }
}
