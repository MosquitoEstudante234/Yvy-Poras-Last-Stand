using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class WaveSpawner : MonoBehaviourPunCallbacks
{
    public static WaveSpawner instance;

    public GameObject[] enemyPrefabs; // Prefabs dos inimigos comuns
    public GameObject bossPrefab; // Prefab do inimigo boss
    public Transform[] spawnPoints; // Locais onde os inimigos podem spawnar
    public TextMeshProUGUI waveText; // Texto para exibir a wave
    public TextMeshProUGUI enemiesText; // Texto para exibir inimigos restantes

    private static bool canWave = false;
    public int waveNumber = 0;
    private int enemiesToSpawn;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
            instance = this;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient && !canWave)
        {
            canWave = true;
            StartWave();
            UpdateUI();
        }
    }

    void StartWave()
    {
        StartNextWave();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isSpawning && enemiesAlive == 0)
        {
            StartNextWave();
        }
    }

    void StartNextWave()
    {
        waveNumber++;
        isSpawning = true;
        UpdateUI();
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(2f); // Pequena pausa antes de iniciar a nova onda

        enemiesToSpawn = waveNumber + Random.Range(1, 3);
        enemiesAlive = 0; // Reset da contagem antes de spawnar

        if (waveNumber % 5 == 0)
        {
            SpawnEnemy(bossPrefab);
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = PhotonNetwork.Instantiate(enemyPrefab.name, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
        UpdateUI();

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeath += EnemyDied;
        }
    }

    void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1); // Garante que nunca fique negativo
        UpdateUI();

        // Debug para checar se o n�mero de inimigos est� correto
        if (enemiesAlive < 0)
        {
            Debug.LogError("Inimigos vivos ficou negativo! Algo est� errado.");
        }
    }

    void UpdateUI()
    {
        waveText.text = "Wave: " + waveNumber;

        enemiesText.text = "Enemies Left: " + enemiesAlive;
    }
}
