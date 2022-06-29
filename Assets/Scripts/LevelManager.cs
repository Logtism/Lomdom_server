using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _singleton;
    public static LevelManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(LevelManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private string[] SceneParts;

    private void Start()
    {
        foreach (string SceneName in SceneParts)
        {
            SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
            Debug.Log($"Loaded scene part \"{SceneName}\"");
        }
    }

    [MessageHandler((ushort)Messages.CTS.openworldloaded)]
    private static void OpenWorldLoaded(ushort fromClientID, Message message)
    {
        foreach (var item in PlayerManager.Singleton.Players)
        {
            Message sync_client_tick = Message.Create(MessageSendMode.reliable, Messages.STC.sync_tick);
            sync_client_tick.AddUInt(NetworkManager.Singleton.CurrentTick);
            NetworkManager.Singleton.Server.Send(sync_client_tick, fromClientID);
            Message createplayer = Message.Create(MessageSendMode.reliable, Messages.STC.create_player);
            createplayer.AddString(item.Value.GetComponent<Player>().Username);
            createplayer.AddUShort(item.Value.GetComponent<Player>().ClientID);
            createplayer.AddVector3(item.Value.transform.position);
            createplayer.AddQuaternion(item.Value.transform.rotation);
            if (item.Value.GetComponent<Player>().ClientID == fromClientID)
            {
                NetworkManager.Singleton.Server.SendToAll(createplayer);
            }
            else
            {
                NetworkManager.Singleton.Server.Send(createplayer, fromClientID);
            }
        }

        AIManager.Singleton.SpawnWaypointAI(AiId.test_civ, new Vector3(0, 1.1f, 0), new Vector3(50, 1.1f, 0), false);
        AIManager.Singleton.SpawnPatrol(AiId.test_civ, new Vector3(0, 1.1f, 0), new List<Vector3>() { new Vector3(45, 0, 45), new Vector3(-45, 0, -45) }, false);
    }

    [MessageHandler((ushort)Messages.CTS.local_player_created)]
    private static void LocalPlayerCreated(ushort fromClientID, Message message)
    {
        Message update_money = Message.Create(MessageSendMode.reliable, Messages.STC.update_money);
        update_money.AddFloat(PlayerManager.Singleton.Players[fromClientID].Balance);
        NetworkManager.Singleton.Server.Send(update_money, fromClientID);
    }
}
