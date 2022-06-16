using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    private static SpawnPoints _singleton;
    public static SpawnPoints Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(SpawnPoints)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] public Transform[] AssassinationSpawnPoints;
    [SerializeField] public Transform[] AssassinationPatrolRoutes;

    public Vector3 GetRandomSpawnPoint(Transform[] spawnpoints)
    {
        Transform point = spawnpoints[Random.Range(0, spawnpoints.Length - 1)];
        return point.position;
    }

    public List<Vector3> GetRandomPatrolRoute(Transform[] partrolpoints)
    {
        Transform route = partrolpoints[Random.Range(0, partrolpoints.Length - 1)];
        List<Vector3> Points = new List<Vector3>();
        foreach (Transform child in route)
        {
            Points.Add(child.position);
        }
        return Points;
    }
}
