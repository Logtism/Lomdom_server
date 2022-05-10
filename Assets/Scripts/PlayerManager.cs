using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _singleton;
    public static PlayerManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(PlayerManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject[] SpawnPoints;

    public Dictionary<ushort, GameObject> Players = new Dictionary<ushort, GameObject>();

    public void CreatePlayer(ushort ClientID, string Username)
    {
        // Getting a random spawn point
        Vector3 spawn_location = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.position;
        // Creating the player gameobject
        Players.Add(ClientID, Instantiate(PlayerPrefab, spawn_location, Quaternion.identity));
        // Setting the ClientID and Username of the player gameobject
        Players[ClientID].GetComponent<Player>().SetPlayerInfo(ClientID, Username);
    }

    public void RemovePlayer(ushort ClientID)
    {
        Destroy(Players[ClientID]);
        Players.Remove(ClientID);
    }
}
