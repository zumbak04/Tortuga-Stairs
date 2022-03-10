using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private List<GameObject> stairsList;
    public GameObject player;
    public int jumps;

    private Vector3 stairsOffset;

    public Text scoreText;
    [System.NonSerialized]
    public int jumpsBeyondScreen = 8;
    [SerializeField]
    private float spawnDelay = 5f;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        stairsList = new List<GameObject>();
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);

        SpawnStairs(stairsOffset);
        SpawnPlayer(new Vector3(0, 0, 0));
        StartCoroutine(SpawnEnemy());
    }
    private void Update()
    {
        float newSpawnDelay = 5f - (float)jumps / 10;
        if(player != null && player.TryGetComponent<Player>(out Player playerScript) && playerScript.IsJumping)
        {
            spawnDelay = Mathf.Max(newSpawnDelay, 0.5f);
        }
        else
        {
            spawnDelay = Mathf.Max(newSpawnDelay, 1f);
        }
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
        scoreText.text = $"—чет: {jumps}";
        if(jumps % jumpsBeyondScreen == 0)
        {
            Vector3 spawnPosition = Vector3.one * jumps + stairsOffset;
            spawnPosition.x = stairsOffset.x;
            SpawnStairs(spawnPosition);
            DestroyStairs(stairsList[0]);
        }
    }
    public IEnumerator SpawnEnemy()
    {
        while (true)
        {
            Vector3 spawnPoint = Vector3.one * (jumps + jumpsBeyondScreen);
            spawnPoint.x = Random.Range(-1, 2);
            Instantiate(GameAssets.instance.enemy, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    public void GameOver()
    {
        Destroy(player);
        player = null;
    }
}
