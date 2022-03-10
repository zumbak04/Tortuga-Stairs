using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : JumpingObject
{
    private int maxJumps = 0;
    [SerializeField]
    private int jumps = 0;

    protected override void Start()
    {
        base.Start();

        maxJumps = GameManager.instance.jumpsBeyondScreen * 2;

        StartCoroutine(JumpDown());
    }

    private IEnumerator JumpDown()
    {
        while (maxJumps > jumps)
        {
            jumps++;
            yield return StartCoroutine(JumpBy(0, -1, -1));
        }
        Destroy(gameObject);
    }
}
