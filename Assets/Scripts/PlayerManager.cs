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

    public Dictionary<ushort, Player> Players = new Dictionary<ushort, Player>();

    public void CreatePlayer(ushort ClientID, string Username)
    {
        // Getting a random spawn point
        Vector3 spawn_location = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.position;
        // Creating the player gameobject
        GameObject player = Instantiate(PlayerPrefab, spawn_location, Quaternion.identity);
        Players.Add(ClientID, player.GetComponent<Player>());
        // Setting the ClientID and Username of the player gameobject
        Players[ClientID].GetComponent<Player>().SetPlayerInfo(ClientID, Username);
    }

    public void RemovePlayer(ushort ClientID)
    {
        Destroy(Players[ClientID].gameObject);
        Players.Remove(ClientID);
    }

    [MessageHandler((ushort)Messages.CTS.inputs)]
    private static void Inputs(ushort fromClientID, Message message)
    {
        if (Singleton.Players.TryGetValue(fromClientID, out Player player))
        {
            player.playermove.SetInputs(message.GetBools(6), message.GetVector3());
        }
    }
}
