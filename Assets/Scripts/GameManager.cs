using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Public Fields
    public static GameManager instance = null;
    public GameObject player;
    public Text scoreText;
    public Button restartButton;
    [System.NonSerialized]
    public int jumpsBeyondScreen = 8;
    [SerializeField]
    #endregion

    #region Private Fields
    private int playerJumps;
    private List<GameObject> stairsList;
    private Vector3 stairsOffset;
    #endregion

    #region Properties
    public int PlayerJumps {
        get => playerJumps;
        set
        {
            playerJumps = value;
            scoreText.text = $"—чет: {playerJumps}";
        }
    }
    public float SpawnDelay
    {
        get
        {
            float spawnDelay = 5f - (float)PlayerJumps / 10;
            if (player != null && player.TryGetComponent<Player>(out Player playerScript) && playerScript.IsJumping)
            {
                spawnDelay = Mathf.Max(spawnDelay, 0.5f);
            }
            else
            {
                spawnDelay = Mathf.Max(spawnDelay, 1f);
            }
            return spawnDelay;
        }
    }
    #endregion

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartGame();
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
    public void MoveForward()
    {
        PlayerJumps++;
        if(PlayerJumps % jumpsBeyondScreen == 0)
        {
            Vector3 spawnPosition = Vector3.one * PlayerJumps + stairsOffset;
            spawnPosition.x = stairsOffset.x;
            SpawnStairs(spawnPosition);
            DestroyStairs(stairsList[0]);
        }
    }
    private IEnumerator SpawnEnemy()
    {
        while (player != null)
        {
            Vector3 spawnPoint = Vector3.one * (PlayerJumps + jumpsBeyondScreen);
            spawnPoint.x = Random.Range(-1, 2);
            Instantiate(GameAssets.instance.enemy, spawnPoint, Quaternion.identity);
            yield return new WaitForSeconds(SpawnDelay);
        }
    }
    public void GameOver()
    {
        Destroy(player);
        restartButton.gameObject.SetActive(true);
    }
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    private void StartGame()
    {
        restartButton.gameObject.SetActive(false);
        stairsList = new List<GameObject>();
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);
        PlayerJumps = 0;

        SpawnStairs(stairsOffset);
        SpawnPlayer(new Vector3(0, 0, 0));
        StartCoroutine(SpawnEnemy());
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }
}
