using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private List<GameObject> stairsList;
    public GameObject player;
    private float moveTime = 0.07f;
    public float inverseMoveTime;
    public int jumps;

    private float playerHeight = -0.2f;
    private Vector3 stairsOffset;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        stairsList = new List<GameObject>();
        stairsOffset = new Vector3(1.5f, -4.5f, -3.5f);
        inverseMoveTime = 1f / moveTime;

        SpawnStairs(stairsOffset);
        SpawnPlayer(new Vector3(0, playerHeight, 0));
    }

    private void SpawnStairs(Vector3 position)
    {
        stairsList.Add(Instantiate(GameAssets.instance.stairs, position, Quaternion.identity));
    }
    private void SpawnPlayer(Vector3 position)
    {
        player = Instantiate(GameAssets.instance.player, position, Quaternion.identity);
    }
    private void DestroyStairs(GameObject stairs)
    {
        stairsList.Remove(stairs);
        Destroy(stairs);
    }
    public void IncreaseJumps()
    {
        jumps++;
        if(jumps % 15 == 0)
        {
            SpawnStairs(player.transform.position + stairsOffset);
            DestroyStairs(stairsList[0]);
        }
    }
}
