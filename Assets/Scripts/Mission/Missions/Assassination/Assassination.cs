using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassination : MonoBehaviour
{
    private static Assassination _singleton;
    public static Assassination Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Assassination)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private Transform[] TargetSpawnLocations;
    [SerializeField] private GameObject TargetPrefab;

    private void Start()
    {

    }
}
