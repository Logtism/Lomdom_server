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
    }
}
