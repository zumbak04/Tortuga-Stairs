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
    public Button startButton;
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
            float spawnDelay = 1f - (float)PlayerJumps / 40;
            if (player != null && player.TryGetComponent<Player>(out Player playerScript) && playerScript.IsJumping)
            {
                spawnDelay = Mathf.Max(spawnDelay, 0.25f);
            }
            else
            {
                spawnDelay = Mathf.Max(spawnDelay, 0.75f);
            }
            return spawnDelay;
        }
    }
    #endregion

    #region Private Methods
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
        InitGame();
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
    private void InitGame()
    {
        stairsList = new List<GameObject>();
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);
        PlayerJumps = 0;

        SpawnStairs(stairsOffset);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }
    #endregion

    #region Public Methods
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
    public void GameOver()
    {
        Destroy(player);
        StopCoroutine("SpawnEnemy");
        restartButton.gameObject.SetActive(true);
    }
    public void StartGame()
    {
        restartButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(true);

        InitGame();
        SpawnPlayer(new Vector3(0, 0, 0));
        StartCoroutine("SpawnEnemy");
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    #endregion
}
