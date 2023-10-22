using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnpoint : MonoBehaviour
{
public GameObject[] enemyPrefabs;
public Transform[] lanePositions;
public float minSpawnDelay = 1.0f;
public float maxSpawnDelay = 3.0f;

public float x = 0f;

private void Start()
{
    StartCoroutine(SpawnEnemies());
}

IEnumerator SpawnEnemies()
{
    while (true)
    {
        yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
        int randomLane = Random.Range(0, lanePositions.Length);
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);

        GameObject newEnemy = Instantiate(enemyPrefabs[randomEnemy], lanePositions[randomLane].position, Quaternion.identity);

        newEnemy.transform.rotation = Quaternion.Euler(0, x, 0);

        newEnemy.AddComponent<enemymovement>();
    }
}
}