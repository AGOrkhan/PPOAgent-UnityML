using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance { get; private set; }

    private Queue<GameObject> obstacles = new Queue<GameObject>();
    private Queue<GameObject> targets = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void InitializePools(GameObject obstaclePrefab, GameObject targetPrefab, int obstaclePoolSize, int targetPoolSize)
    {
        for (int i = 0; i < obstaclePoolSize; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefab);
            obstacle.SetActive(false);
            obstacles.Enqueue(obstacle);
        }

        for (int i = 0; i < targetPoolSize; i++)
        {
            GameObject target = Instantiate(targetPrefab); 
            target.SetActive(false);
            targets.Enqueue(target);
        }
    }

    public GameObject GetObstacle()
    {
        if (obstacles.Count > 0)
        {
            GameObject obstacle = obstacles.Dequeue();
            obstacle.SetActive(true);
            return obstacle;
        }
        else
        {
            throw new Exception("Obstacle pool is empty");
        }
    }

    public GameObject GetTarget(GameObject targetPrefab, int targetAmount)
    {
        if (targets.Count > 0)
        {
            GameObject target = targets.Dequeue();
            target.SetActive(true);
            return target;
        }
        else
        {
            throw new Exception("Target pool is empty");
        }
    }

    public void ReturnObstacle(GameObject obstacle)
    {
        // Reset the scale of the obstacle to its original size
        obstacle.transform.localScale = Vector3.one;
        obstacle.SetActive(false);
        obstacles.Enqueue(obstacle);
    }

    public void ReturnTarget(GameObject target)
    {
        target.SetActive(false);
        targets.Enqueue(target);
    }
}