using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Prefabs dos inimigos comuns
    public GameObject bossPrefab; // Prefab do inimigo boss
    public Transform[] spawnPoints; // Locais onde os inimigos podem spawnar
    public TextMeshProUGUI waveText; // Texto para exibir a wave
    public TextMeshProUGUI enemiesText; // Texto para exibir inimigos restantes

    public int waveNumber = 0;
    private int enemiesToSpawn;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        if (!isSpawning && enemiesAlive <= 0)
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
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
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
        enemiesAlive--;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = "Wave: " + waveNumber;

        if (enemiesText != null)
            enemiesText.text = "Enemies Left: " + enemiesAlive;
    }
}