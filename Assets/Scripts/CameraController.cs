using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;
    void Start()
    {
        offset = new Vector3(2.4f, 7.3f, -0.6f);
    }

    void Update()
    {
        transform.position = GameManager.instance.player.transform.position + offset;
    }
}
