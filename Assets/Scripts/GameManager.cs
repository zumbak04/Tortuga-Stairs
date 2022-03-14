using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using static Firebase.Extensions.TaskExtension;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    #region Public Fields
    public static GameManager instance = null;
    public GameObject player;
    public Text scoreText;
    public Button restartButton;
    public Button startButton;
    public InputField nameField;
    public Text leaderboardText;
    [System.NonSerialized]
    public int jumpsBeyondScreen = 8;
    [SerializeField]
    #endregion

    #region Private Fields
    private int playerJumps;
    private List<GameObject> stairsList;
    private Vector3 stairsOffset;
    private DatabaseReference leaderboard;
    #endregion

    #region Properties
    public int PlayerJumps {
        get => playerJumps;
        set
        {
            playerJumps = value;
            scoreText.text = $"Счет: {playerJumps}";
        }
    }
    public float SpawnDelay
    {
        get
        {
            float spawnDelay = Mathf.Max(1f - (float)PlayerJumps / 40, 0);
            spawnDelay = Mathf.Max(spawnDelay, 0.6f);

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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception is null)
            {
                leaderboard = FirebaseDatabase.DefaultInstance.GetReference("Leaders");
            }
            else
            {
                Debug.LogError("Firebase не инициализирована");
            }
        });

        SceneManager.sceneLoaded += OnSceneLoaded;
        stairsOffset = new Vector3(1.5f, -4.3f, -3.5f);
        stairsList = new List<GameObject>();
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
            spawnPoint.x = UnityEngine.Random.Range(-1, 2);
            if (!Physics.CheckSphere(spawnPoint, 0.8f))
            {
                Instantiate(GameAssets.instance.enemy, spawnPoint, Quaternion.identity);
            }
            yield return new WaitForSeconds(SpawnDelay);
        }
    }
    private void InitGame()
    {
        nameField.gameObject.SetActive(false);
        leaderboardText.gameObject.SetActive(false);

        stairsList.Clear();
        PlayerJumps = 0;

        SpawnStairs(stairsOffset);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }
    private void ShowLeaderboard()
    {
        leaderboardText.gameObject.SetActive(true);
        UpdateLeaderboard();
    }
    private void UpdateLeaderboard()
    {
        if(!(leaderboard is null))
        {
            leaderboardText.text = "Лидеры:\n";
            leaderboard.OrderByValue().LimitToLast(3).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.ChildrenCount > 0)
                    {
                        var reversedLeaders = snapshot.Children.Reverse();
                        foreach (DataSnapshot leader in reversedLeaders)
                        {
                            leaderboardText.text += $"{leader.Key}: {leader.Value}\n";
                        }
                    }
                }
            });
        }
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
        if(!(leaderboard is null))
        {
            nameField.gameObject.SetActive(true);
            ShowLeaderboard();
        }
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
    public void EnterName()
    {
        if(!(leaderboard is null))
        {
            leaderboard.Child(nameField.text).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (Convert.ToInt32(snapshot.Value) < PlayerJumps)
                    {
                        //Какого хуя?
                        leaderboard.Child(nameField.text).SetValueAsync(PlayerJumps);
                    }
                }
                else
                {
                    //Какого хуя?
                    leaderboard.Child(nameField.text).SetValueAsync(PlayerJumps);
                }
            });

            UpdateLeaderboard();
        }

        nameField.gameObject.SetActive(false);
    }
    #endregion
}
