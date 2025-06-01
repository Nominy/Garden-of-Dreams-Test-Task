using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Collider2D spawnArea;
    public int numberOfMonstersToSpawn = 10;

    void Awake()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("Monster Prefab not assigned!");
            return;
        }

        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area not assigned!");
            return;
        }

        for (int i = 0; i < numberOfMonstersToSpawn; i++)
        {
            Bounds bounds = spawnArea.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);

            Vector2 spawnPosition2D = new Vector2(randomX, randomY);
            Vector3 spawnPosition = new Vector3(spawnPosition2D.x, spawnPosition2D.y, transform.position.z);

            if (spawnArea.ClosestPoint(spawnPosition2D) == spawnPosition2D)
            {
                Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Vector2 closestPoint2D = spawnArea.ClosestPoint(spawnPosition2D);
                Vector3 closestPoint3D = new Vector3(closestPoint2D.x, closestPoint2D.y, transform.position.z);
                Instantiate(monsterPrefab, closestPoint3D, Quaternion.identity);
            }
        }
    }

}
